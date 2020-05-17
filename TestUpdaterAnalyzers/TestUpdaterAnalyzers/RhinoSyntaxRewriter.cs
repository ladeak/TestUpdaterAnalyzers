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
        private SemanticModel _originalSemantics;
        private SyntaxWalkContext<InvocationFixContextData> _invocationContext = new SyntaxWalkContext<InvocationFixContextData>();
        private SyntaxWalkContext<MethodFixContextData> _methodContext = new SyntaxWalkContext<MethodFixContextData>();

        public RhinoSyntaxRewriter(SemanticModel semanticModel)
        {
            _originalSemantics = semanticModel;
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
                node = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;
                return CompleteVerifyAllStatements(node);
            }
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            using (_invocationContext.Enter())
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
                            invocationExpr = invocationExpr.WithArgumentList(ReWriteOutRefArguments(invocationExpr));
                            if (_invocationContext.Current.UseAnyArgs)
                                return invocationExpr.WithExpression(UseReturnsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseReturns(memberAccessExpr));

                        }
                        if (RhinoRecognizer.IsExpectMethod(originalMemberSymbol) || RhinoRecognizer.IsStubMethod(originalMemberSymbol))
                        {
                            return DropExpectOrStubCall(memberAccessExpr);
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
                            _invocationContext.Current.UseAnyArgs = true;
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
                            _invocationContext.Current.OutRefArguments.AddRange(invocationExpr.ArgumentList.Arguments.Select(x => x.Expression));
                            return outRefInnerInvocationExpression;
                        }
                        if (RhinoRecognizer.IsVerifyAllExpectationsMethod(originalMemberSymbol))
                        {
                            return UseReceivedOnMethodContext(invocationExpr);
                        }
                    }
                }
                return invocationExpr;
            }
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            var argumentExpr = node.Expression as MemberAccessExpressionSyntax;
            if (argumentExpr != null)
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
            return base.VisitArgument(node);
        }



        private SyntaxNode DropExpectOrStubCall(MemberAccessExpressionSyntax parentNode)
        {
            var mockedObjectIdentifier = parentNode.Expression as IdentifierNameSyntax;

            var expectInvocationExpression = parentNode.Parent as InvocationExpressionSyntax;
            if (expectInvocationExpression == null)
                return parentNode.Parent;
            var argumentLambda = expectInvocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression as LambdaExpressionSyntax;
            var mockMethodInvocation = argumentLambda.Body as InvocationExpressionSyntax;
            if (!(mockMethodInvocation?.Expression is MemberAccessExpressionSyntax mockedMethod))
                return parentNode.Parent;

            var invocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                mockedObjectIdentifier, mockedMethod.Name), mockMethodInvocation.ArgumentList);

            _invocationContext.Current.OriginalArguments.AddRange(mockMethodInvocation.ArgumentList.Arguments);

            if (parentNode.Name.Identifier.ValueText == "Expect")
            {
                var receivedInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    mockedObjectIdentifier, SyntaxFactory.IdentifierName("Received")));
                var assertInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    receivedInvocation, mockedMethod.Name), mockMethodInvocation.ArgumentList);
                _methodContext.Current.Add(mockedObjectIdentifier.Identifier.ValueText, assertInvocation);
            }
            return invocation;
        }

        private MemberAccessExpressionSyntax UseReturns(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("Returns"));
        }

        private MemberAccessExpressionSyntax UseReturnsForAnyArgs(MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.WithName(SyntaxFactory.IdentifierName("ReturnsForAnyArgs"));
        }

        private ArgumentListSyntax ReWriteOutRefArguments(InvocationExpressionSyntax invocation)
        {
            if (!_invocationContext.Current.OutRefArguments.Any()
                || !_invocationContext.Current.OriginalArguments.Any())
                return invocation.ArgumentList;

            var paramToken = SyntaxFactory.ParseToken("c");

            int currentOutArgumentIndex = 0;
            List<StatementSyntax> statements = new List<StatementSyntax>();
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
            statements.Add(SyntaxFactory.ReturnStatement(invocation.ArgumentList.Arguments.First().Expression));

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

        private SyntaxNode CompleteVerifyAllStatements(MethodDeclarationSyntax node)
        {
            var statements = node.Body.Statements;
            foreach (var receivedCall in _methodContext.Current.TakeRest())
                statements = statements.Add(SyntaxFactory.ExpressionStatement(receivedCall));
            node = node.WithBody(node.Body.WithStatements(statements));
            return node;
        }

        public SyntaxNode UseArgsAny(ArgumentSyntax argument, bool outParam = false)
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


    }
}
