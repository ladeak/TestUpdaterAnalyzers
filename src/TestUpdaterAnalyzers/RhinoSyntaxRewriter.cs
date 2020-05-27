using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestUpdaterAnalyzers
{
    public class RhinoSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _originalSemantics;
        private readonly SyntaxWalkContext<InvocationFixContextData, InvocationExpressionSyntax> _invocationContext;
        private readonly SyntaxWalkContext<MethodFixContextData> _methodContext;

        public RhinoSyntaxRewriter(SemanticModel semanticModel)
        {
            _originalSemantics = semanticModel;
            _invocationContext = new SyntaxWalkContext<InvocationFixContextData, InvocationExpressionSyntax>(InitializeInvocationState);
            _methodContext = new SyntaxWalkContext<MethodFixContextData>();
        }

        public SyntaxNode Rewrite(SyntaxNode node)
        {
            return Visit(node);
        }

        public bool UseExceptionExtensions { get; private set; }

        public bool UseReceivedExtensions { get; private set; }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            using (_methodContext.Enter())
            {
                FindEmptySyntaxToken(node);
                node = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;
                return CompleteVerifyAllStatements(node);
            }
        }

        private void FindEmptySyntaxToken(MethodDeclarationSyntax node)
        {
            int i = 0;
            while (node.DescendantTokens().Any(x => x.IsKind(SyntaxKind.IdentifierToken) && x.ValueText == _methodContext.Current.UnusedLambdaToken.ValueText))
                _methodContext.Current.UnusedLambdaToken = SyntaxFactory.Identifier($"a{++i}");
        }

        private InvocationFixContextData InitializeInvocationState(InvocationExpressionSyntax invocationExpr)
        {
            var builder = new RhinoSyntaxInvocationStateBuilder(_originalSemantics);
            return builder.Build(invocationExpr);
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            using (_invocationContext.Enter(invocationExpr))
            {
                var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
                if (memberAccessExpr != null)
                {
                    var originalSymbolInfo = _originalSemantics.GetSymbolInfo(memberAccessExpr);

                    invocationExpr = base.VisitInvocationExpression(invocationExpr) as InvocationExpressionSyntax;
                    memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

                    var originalMemberSymbol = originalSymbolInfo.Symbol as IMethodSymbol ?? originalSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                    if (originalMemberSymbol != null)
                    {
                        if (RhinoRecognizer.IsReturnMethod(originalMemberSymbol))
                        {
                            invocationExpr = invocationExpr.WithArgumentList(ReWriteArguments(invocationExpr));
                            if (_invocationContext.Current.UseAnyArgs)
                                return invocationExpr.WithExpression(UseReturnsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseReturns(memberAccessExpr));

                        }
                        if (RhinoRecognizer.IsExpectMethod(originalMemberSymbol) || RhinoRecognizer.IsStubMethod(originalMemberSymbol))
                        {
                            return ExtractExpectAndStubInvocation(memberAccessExpr);
                        }
                        if (RhinoRecognizer.IsGenerateMockMethod(originalMemberSymbol) || RhinoRecognizer.IsGenerateStubMethod(originalMemberSymbol))
                        {
                            return UseSubstituteFor(memberAccessExpr);
                        }
                        if (RhinoRecognizer.IsThrowMethod(originalMemberSymbol))
                        {
                            UseExceptionExtensions = true;
                            if (_invocationContext.Current.UseAnyArgs)
                                return invocationExpr.WithExpression(UseThrowsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseThrows(memberAccessExpr));
                        }
                        if (RhinoRecognizer.IsIgnoreArgumentsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax ignoreArgumentsMemberExpression
                            && ignoreArgumentsMemberExpression.Expression is InvocationExpressionSyntax innerIgnoreArgumentsInvocationExpression)
                        {
                            return innerIgnoreArgumentsInvocationExpression;
                        }
                        if (RhinoRecognizer.IsAnyRepeatOptionsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax repeatOptionMemberAccess
                            && repeatOptionMemberAccess.Expression is MemberAccessExpressionSyntax repeatMemberAccess
                            && repeatMemberAccess.Expression != null)
                        {
                            return repeatMemberAccess.Expression;
                        }
                        if (RhinoRecognizer.IsOutRefProperty(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax outRefMemberExpression
                            && outRefMemberExpression.Expression is InvocationExpressionSyntax outRefInnerInvocationExpression)
                        {
                            return outRefInnerInvocationExpression;
                        }
                        if (RhinoRecognizer.IsVerifyAllExpectationsMethod(originalMemberSymbol))
                        {
                            return UseReceivedOnMethodContext(invocationExpr);
                        }
                        if (RhinoRecognizer.IsAssertWasCalledMethod(originalMemberSymbol))
                        {
                            return ExtractAssertCalledInvocation(memberAccessExpr, "AssertWasCalled", "Received");
                        }
                        if (RhinoRecognizer.IsAssertWasNotCalledMethod(originalMemberSymbol))
                        {
                            return ExtractAssertCalledInvocation(memberAccessExpr, "AssertWasNotCalled", "DidNotReceive");
                        }
                        if (RhinoRecognizer.IsPropertyBehavior(originalMemberSymbol))
                        {
                            return RemoveInvocation(invocationExpr);
                        }
                        if (RhinoRecognizer.IsWhenCalledMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax whenCalledMemberAccess
                            && invocationExpr.ArgumentList.Arguments.FirstOrDefault().Expression is SimpleLambdaExpressionSyntax whenCalledLambda)
                        {
                            return UseInnerCallOrWhenDo(invocationExpr, whenCalledMemberAccess);
                        }
                    }
                }
                return invocationExpr;
            }
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            if (node.Expression is MemberAccessExpressionSyntax argumentExpr)
            {
                var argumentSymbol = _originalSemantics.GetSymbolInfo(argumentExpr).Symbol;
                if (argumentSymbol is IPropertySymbol propertySymbol)
                {
                    if (RhinoRecognizer.IsAnythingProperty(propertySymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.IsIsArgProperty(innerSymbol))
                        {
                            return UseArgsAny(node);
                        }
                    }
                    if (RhinoRecognizer.IsNullArgProperty(propertySymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.IsIsArgProperty(innerSymbol))
                        {
                            return UseArgWith(node, GetEqualsNullArgument(_methodContext.Current.UnusedLambdaToken));
                        }
                    }
                    if (RhinoRecognizer.IsNotNullArgProperty(propertySymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.IsIsArgProperty(innerSymbol))
                        {
                            return UseArgWith(node, GetNotEqualsNullArgument(_methodContext.Current.UnusedLambdaToken));
                        }
                    }
                }

                if (node.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) && argumentSymbol is IFieldSymbol fieldSymbol)
                {
                    if (RhinoRecognizer.IsDummyField(fieldSymbol))
                    {
                        var outMethodSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IMethodSymbol;
                        if (RhinoRecognizer.IsOutArgMethod(outMethodSymbol) && argumentExpr.Expression is InvocationExpressionSyntax outMethodInvocation)
                        {
                            _invocationContext.Current.OutRefArguments.Add(outMethodInvocation.ArgumentList.Arguments.First().Expression);
                            return UseArgsAny(node, true);
                        }
                    }
                }
            }

            if (node.Expression is InvocationExpressionSyntax argumentMethodInvocation)
            {
                var argumentSymbol = _originalSemantics.GetSymbolInfo(argumentMethodInvocation.Expression).Symbol;
                if (argumentSymbol is IMethodSymbol methodSymbol)
                {
                    if (RhinoRecognizer.IsEqualArgMethod(methodSymbol))
                    {
                        var equalsTo = argumentMethodInvocation.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax;
                        return UseArgWith(node, GetEqualsToGivenArgument(_methodContext.Current.UnusedLambdaToken, equalsTo));
                    }
                    if (RhinoRecognizer.IsSameArgMethod(methodSymbol))
                    {
                        var sameTo = argumentMethodInvocation.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax;
                        return UseArgWith(node, GetReferenceEqualsToGivenArgument(_methodContext.Current.UnusedLambdaToken, sameTo));
                    }
                    if (RhinoRecognizer.IsMatchesArgMethod(methodSymbol))
                    {
                        var lambdaArgument = argumentMethodInvocation.ArgumentList.Arguments.First().Expression as SimpleLambdaExpressionSyntax;
                        return UseArgWith(node, GetLambdaAsArgument(_methodContext.Current.UnusedLambdaToken, lambdaArgument));
                    }
                }
            }

            return base.VisitArgument(node);
        }

        private SyntaxNode ExtractExpectAndStubInvocation(MemberAccessExpressionSyntax memberAccess)
        {
            (IdentifierNameSyntax mockedObjectIdentifier, ArgumentListSyntax argumentList, ExpressionSyntax lambdaBody) = ExtractLambdaToParts(memberAccess);
            if (mockedObjectIdentifier == null)
                return memberAccess.Parent;

            // If Expect call, generate a syntax for VerifyAllExpectations calls. Filter out property getters as MemberAccessExpression
            if (memberAccess.Name.Identifier.ValueText == "Expect" && !(lambdaBody is MemberAccessExpressionSyntax))
            {
                (var assertInvocation, var assertKey) = PrepandCallToInvocation(mockedObjectIdentifier, "Received", lambdaBody);
                _methodContext.Current.Add(assertKey, assertInvocation);
            }

            // If it is an empty Expect or Stub invocation, add removable statements.
            if (memberAccess.Parent.Parent is ExpressionStatementSyntax && memberAccess.Parent is InvocationExpressionSyntax removableInvocation)
            {
                RemoveInvocation(removableInvocation);
                return memberAccess.Parent;
            }

            // Has parent, but not Returns or Throws (ie. WhenCalled)
            if (!_invocationContext.Current.HasReturn && !_invocationContext.Current.HasThrow && _invocationContext.Current.WhenCalledLambda != null
                && memberAccess.Parent is InvocationExpressionSyntax parentInvocation)
            {
                string whenMethod = _invocationContext.Current.UseAnyArgs ? "WhenForAnyArgs" : "When";
                return parentInvocation.WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(whenMethod)));
            }

            // Else create new invocation
            ExpressionSyntax invocation = lambdaBody;
            return invocation;
        }

        private SyntaxNode ExtractAssertCalledInvocation(MemberAccessExpressionSyntax parentNode, string prepandInvocationFilter, string prepandCall)
        {
            (IdentifierNameSyntax mockedObjectIdentifier, ArgumentListSyntax arguments, ExpressionSyntax lambdaBody) = ExtractLambdaToParts(parentNode);
            if (mockedObjectIdentifier == null)
                return parentNode.Parent;

            if (parentNode.Name.Identifier.ValueText == prepandInvocationFilter)
            {
                (var fullInvocation, _) = PrepandCallToInvocation(mockedObjectIdentifier, prepandCall, lambdaBody);
                return fullInvocation;
            }
            return parentNode.Parent;
        }

        private (IdentifierNameSyntax mockedObjectIdentifier, ArgumentListSyntax arguments, ExpressionSyntax lambdaBody) ExtractLambdaToParts(MemberAccessExpressionSyntax parentNode)
        {
            var mockedObjectIdentifier = parentNode.Expression as IdentifierNameSyntax;

            var expectInvocationExpression = parentNode.Parent as InvocationExpressionSyntax;
            if (expectInvocationExpression == null)
                return (null, null, null);
            var argumentLambda = expectInvocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression as SimpleLambdaExpressionSyntax;

            var param = argumentLambda.Parameter.Identifier;
            var tokens = argumentLambda.DescendantTokens().Where(x => x.Kind() == param.Kind() && x.ValueText == param.ValueText).ToList();
            var renamedLambda = argumentLambda.ReplaceTokens(tokens, (_, __) => mockedObjectIdentifier.Identifier);

            ArgumentListSyntax arguments = null;
            var mockMethodInvocation = renamedLambda.Body as InvocationExpressionSyntax; // A method is mocked, attach arguments for out params processing.
            if (mockMethodInvocation?.Expression is MemberAccessExpressionSyntax mockedMethod)
                arguments = mockMethodInvocation.ArgumentList;
            return (mockedObjectIdentifier, arguments, renamedLambda.Body as ExpressionSyntax);
        }

        private (ExpressionSyntax fullInvocation, string identifierToken) PrepandCallToInvocation(IdentifierNameSyntax mockedObjectIdentifier, string prepandCall, ExpressionSyntax lambdaBody)
        {
            var prependInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                mockedObjectIdentifier, SyntaxFactory.IdentifierName(prepandCall)));
            var nameToken = lambdaBody.DescendantNodes().First(x => x.Kind() == mockedObjectIdentifier.Kind() && ((IdentifierNameSyntax)x).Identifier.ValueText == mockedObjectIdentifier.Identifier.ValueText);

            var fullInvocation = lambdaBody.ReplaceNode(nameToken, prependInvocation);
            return (fullInvocation, mockedObjectIdentifier.Identifier.ValueText);
        }

        private MemberAccessExpressionSyntax UseReturns(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("Returns"));
        }

        private MemberAccessExpressionSyntax UseReturnsForAnyArgs(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("ReturnsForAnyArgs"));
        }

        private ArgumentListSyntax ReWriteArguments(InvocationExpressionSyntax invocation)
        {
            if ((!_invocationContext.Current.OutRefArguments.Any()
                || !_invocationContext.Current.OriginalArguments.Any())
                && _invocationContext.Current.WhenCalledLambda is null)
                return invocation.ArgumentList;

            var paramToken = _methodContext.Current.UnusedLambdaToken;

            int currentOutArgumentIndex = 0;
            List<StatementSyntax> statements = new List<StatementSyntax>();
            ExpressionSyntax returnExpression = invocation.ArgumentList.Arguments.First().Expression;

            // Exectute WhenCalledStatements
            if (_invocationContext.Current.WhenCalledLambda?.Body is BlockSyntax lambdaBlock)
            {
                var methodInvocationRewriter = new MethodInvocationLambdaRewriter(_invocationContext.Current.WhenCalledLambda.Parameter, paramToken);
                foreach (var statement in lambdaBlock.Statements)
                {
                    if (methodInvocationRewriter.Rewrite(statement, out var newSyntax) && newSyntax is StatementSyntax rewrittenStatement)
                        statements.Add(rewrittenStatement);
                }
                if (methodInvocationRewriter.ReturnStatement != null)
                    returnExpression = methodInvocationRewriter.ReturnStatement;
            }

            // Set Out Arguments on CallInfo
            foreach (var outArg in _invocationContext.Current.OutRefArguments)
            {
                currentOutArgumentIndex = _invocationContext.Current.OriginalArguments.FindIndex(currentOutArgumentIndex, x => x.RefKindKeyword.IsKind(SyntaxKind.OutKeyword));
                var indexToken = SyntaxFactory.ParseToken(currentOutArgumentIndex.ToString());
                var outParamAssignment = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                      SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(paramToken),
                      SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                          SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, indexToken))))),
                      outArg));
                statements.Add(outParamAssignment);
            }

            // Add return statement
            statements.Add(SyntaxFactory.ReturnStatement(returnExpression));

            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.SimpleLambdaExpression(
                        SyntaxFactory.Parameter(paramToken),
                        SyntaxFactory.Block(statements.ToArray())
                     )
                  )
               )
            );
        }

        private MemberAccessExpressionSyntax UseThrows(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("Throws"));
        }

        private MemberAccessExpressionSyntax UseThrowsForAnyArgs(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("ThrowsForAnyArgs"));
        }

        private ExpressionSyntax UseInnerCallOrWhenDo(InvocationExpressionSyntax invocationExpr, MemberAccessExpressionSyntax whenCalledMemberAccess)
        {
            if (_invocationContext.Current.HasReturn || _invocationContext.Current.HasThrow)
                return whenCalledMemberAccess.Expression;
            else
                return invocationExpr.WithExpression(whenCalledMemberAccess.WithName(SyntaxFactory.IdentifierName("Do")));
        }

        private SyntaxNode UseSubstituteFor(MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Name is GenericNameSyntax generateMockIdentifier)
            {
                var typeArguments = generateMockIdentifier.TypeArgumentList;

                var newInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Substitute"),
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("For"), typeArguments)));

                return newInvocation;
            }
            return memberAccess.Parent;
        }

        private SyntaxNode UseReceivedOnMethodContext(InvocationExpressionSyntax verifyInvocationNode)
        {
            if (verifyInvocationNode.Expression is MemberAccessExpressionSyntax verifyMemberAccess)
            {
                if (verifyMemberAccess.Expression is IdentifierNameSyntax mockIdentifier)
                {
                    var token = mockIdentifier.Identifier;
                    if (_methodContext.Current.TakeFirst(token.ValueText, out var firstInvocation))
                        return firstInvocation;
                }
            }
            return verifyInvocationNode;
        }

        private SyntaxNode RemoveInvocation(InvocationExpressionSyntax invocationExpression)
        {
            _methodContext.Current.RemovableExpressions.Add(invocationExpression);
            return invocationExpression;
        }

        private SyntaxNode CompleteVerifyAllStatements(MethodDeclarationSyntax node)
        {
            var statements = node.Body.Statements;
            foreach (var removable in _methodContext.Current.RemovableExpressions)
            {
                var removableText = removable.ToString();
                var expressionStatementToRemove = statements.OfType<ExpressionStatementSyntax>().FirstOrDefault(x => x.Expression is InvocationExpressionSyntax && x.Expression.ToString() == removableText);
                statements = statements.Remove(expressionStatementToRemove);
            }

            foreach (var receivedCall in _methodContext.Current.TakeRest())
                statements = statements.Add(SyntaxFactory.ExpressionStatement(receivedCall));
            node = node.WithBody(node.Body.WithStatements(statements));
            return node;
        }

        private SyntaxNode UseArgsAny(ArgumentSyntax argument, bool outParam = false)
        {
            if (argument.Expression is MemberAccessExpressionSyntax finalMemberAccess)
            {
                var beginExpression = finalMemberAccess.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
                    InvocationExpressionSyntax invocationExpression => (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Expression,
                    _ => null
                };

                if (beginExpression is GenericNameSyntax argGenericArgument)
                {
                    var typeArguments = argGenericArgument.TypeArgumentList;
                    var newArg = SyntaxFactory.Argument(
                        SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("NSubstitute"),
                        SyntaxFactory.IdentifierName("Arg")),
                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("Any"), typeArguments))));

                    if (outParam)
                        newArg = newArg.WithRefKindKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword));
                    return newArg;
                }
            }
            return argument;
        }

        private SyntaxNode UseArgWith(ArgumentSyntax argument, ArgumentListSyntax lambdaArgument)
        {
            ExpressionSyntax beginExpression = null;
            var finalExpression = argument.Expression;

            if (finalExpression is InvocationExpressionSyntax invocationExpr) // It is Equal() or Same method() call
                finalExpression = invocationExpr.Expression;

            if (finalExpression is MemberAccessExpressionSyntax finalMemberAccess)
            {
                beginExpression = finalMemberAccess.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
                    InvocationExpressionSyntax invocationExpression => (invocationExpression.Expression as MemberAccessExpressionSyntax)?.Expression, // out Dummy argument
                    _ => finalMemberAccess.Expression // Arg.Matches
                };
            }

            if (beginExpression is GenericNameSyntax argGenericArgument)
            {
                var typeArguments = argGenericArgument.TypeArgumentList;
                var newArg = SyntaxFactory.Argument(
                    SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("NSubstitute"),
                    SyntaxFactory.IdentifierName("Arg")),
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("Is"), typeArguments)), lambdaArgument));
                return newArg;
            }

            return argument;
        }

        private ArgumentListSyntax GetEqualsNullArgument(SyntaxToken token)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(token),
                SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, SyntaxFactory.IdentifierName(token), SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))))));
        }

        private ArgumentListSyntax GetNotEqualsNullArgument(SyntaxToken token)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(token),
                    SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, SyntaxFactory.IdentifierName(token), SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))))));
        }

        private ArgumentListSyntax GetEqualsToGivenArgument(SyntaxToken token, IdentifierNameSyntax equalsTo)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(token),
                    SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, SyntaxFactory.IdentifierName(token), equalsTo)))));
        }

        private ArgumentListSyntax GetReferenceEqualsToGivenArgument(SyntaxToken token, IdentifierNameSyntax sameAs)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(
                    SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(token),
                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("ReferenceEquals"),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(SyntaxFactory.IdentifierName(token)), SyntaxFactory.Argument(sameAs) }))
            )))));
        }

        private ArgumentListSyntax GetLambdaAsArgument(SyntaxToken token, SimpleLambdaExpressionSyntax lambdaArgument)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(lambdaArgument)));
        }

    }
}
