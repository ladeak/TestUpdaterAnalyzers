using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestUpdaterAnalyzers
{
    public class RhinoSyntaxInvocationStateBuilder : CSharpSyntaxWalker
    {
        private SemanticModel _originalSemantics;
        private SyntaxWalkContext<InvocationFixContextData> _invocationContext = new SyntaxWalkContext<InvocationFixContextData>();

        public RhinoSyntaxInvocationStateBuilder(SemanticModel semanticModel)
        {
            _originalSemantics = semanticModel;
        }

        public InvocationFixContextData Build(InvocationExpressionSyntax node)
        {
            using (_invocationContext.Enter())
            {
                Visit(node);
                return _invocationContext.Current;
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            using (_invocationContext.Enter())
            {
                var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
                if (memberAccessExpr != null)
                {
                    var originalSymbolInfo = _originalSemantics.GetSymbolInfo(memberAccessExpr);

                    base.VisitInvocationExpression(invocationExpr);
                    memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

                    var originalMemberSymbol = originalSymbolInfo.Symbol as IMethodSymbol ?? originalSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                    if (originalMemberSymbol != null)
                    {
                        if (RhinoRecognizer.IsReturnMethod(originalMemberSymbol))
                        {
                            _invocationContext.Current.HasReturn = true;
                            return;
                        }
                        if (RhinoRecognizer.IsExpectMethod(originalMemberSymbol) || RhinoRecognizer.IsStubMethod(originalMemberSymbol))
                        {
                            ExtractExpectAndStubInvocation(memberAccessExpr);
                            return;
                        }
                        if (RhinoRecognizer.IsGenerateMockMethod(originalMemberSymbol) || RhinoRecognizer.IsGenerateStubMethod(originalMemberSymbol))
                        {
                            return;
                        }
                        if (RhinoRecognizer.IsThrowMethod(originalMemberSymbol))
                        {
                            _invocationContext.Current.UseExceptionExtensions = true;
                            _invocationContext.Current.HasThrow = true;
                            return;
                        }
                        if (RhinoRecognizer.IsIgnoreArgumentsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax ignoreArgumentsMemberExpression
                            && ignoreArgumentsMemberExpression.Expression is InvocationExpressionSyntax innerIgnoreArgumentsInvocationExpression)
                        {
                            _invocationContext.Current.UseAnyArgs = true;
                            return;
                        }
                        if (RhinoRecognizer.IsAnyRepeatOptionsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax repeatOptionMemberAccess
                            && repeatOptionMemberAccess.Expression is MemberAccessExpressionSyntax repeatMemberAccess
                            && repeatMemberAccess.Expression != null)
                        {
                            return;
                        }
                        if (RhinoRecognizer.IsOutRefProperty(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax outRefMemberExpression
                            && outRefMemberExpression.Expression is InvocationExpressionSyntax outRefInnerInvocationExpression)
                        {
                            _invocationContext.Current.OutRefArguments.AddRange(invocationExpr.ArgumentList.Arguments.Select(x => x.Expression));
                            return;
                        }
                        if (RhinoRecognizer.IsVerifyAllExpectationsMethod(originalMemberSymbol))
                        {
                            return;
                        }
                        if (RhinoRecognizer.IsAssertWasCalledMethod(originalMemberSymbol))
                        {
                            return;
                        }
                        if (RhinoRecognizer.IsAssertWasNotCalledMethod(originalMemberSymbol))
                        {
                            return;
                        }
                        if (RhinoRecognizer.IsPropertyBehavior(originalMemberSymbol))
                        {
                            RemoveInvocation(invocationExpr);
                            return;
                        }
                        if (RhinoRecognizer.IsWhenCalledMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax whenCalledMemberAccess
                            && invocationExpr.ArgumentList.Arguments.FirstOrDefault().Expression is SimpleLambdaExpressionSyntax whenCalledLambda)
                        {
                            _invocationContext.Current.WhenCalledLambda = whenCalledLambda;
                            return;
                        }
                    }
                }
            }
        }

        private void ExtractExpectAndStubInvocation(MemberAccessExpressionSyntax parentNode)
        {
            (IdentifierNameSyntax mockedObjectIdentifier, ArgumentListSyntax argumentList, ExpressionSyntax lambdaBody) = ExtractLambdaToParts(parentNode);
            if (mockedObjectIdentifier == null)
                return;

            // If Expect call, generate a syntax for VerifyAllExpectations calls. Filter out property getters as MemberAccessExpression
            if (parentNode.Name.Identifier.ValueText == "Expect" && !(lambdaBody is MemberAccessExpressionSyntax))
            {
                (var assertInvocation, var assertKey) = PrepandCallToInvocation(mockedObjectIdentifier, "Received", lambdaBody);
                _invocationContext.Current.ExpectCallForAssertion = new KeyValuePair<string, ExpressionSyntax>(assertKey, assertInvocation);
            }

            // If it is an empty Expect or Stub invocation, add removable statements.
            if (parentNode.Parent.Parent is ExpressionStatementSyntax && parentNode.Parent is InvocationExpressionSyntax removableInvocation)
            {
                RemoveInvocation(removableInvocation);
                return;
            }

            // Else create new invocation
            if (argumentList != null)
                _invocationContext.Current.OriginalArguments.AddRange(argumentList.Arguments);

            return;
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

        private SyntaxNode RemoveInvocation(InvocationExpressionSyntax invocationExpression)
        {
            _invocationContext.Current.IsRemovable = invocationExpression;
            return invocationExpression;
        }
    }
}
