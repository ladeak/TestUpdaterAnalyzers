using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestUpdaterAnalyzers
{
    public class RhinoInvocationSyntaxRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel _originalSemantics;
        private SyntaxWalkContext<InvocationData> _currentInvocationContext = new SyntaxWalkContext<InvocationData>();

        public RhinoInvocationSyntaxRewriter(SemanticModel semanticModel)
        {
            _originalSemantics = semanticModel;
        }

        public SyntaxNode Rewrite(SyntaxNode node)
        {
            return Visit(node);
        }

        public bool UseExceptionExtensions { get; private set; }

        private class InvocationData
        {
            public bool UseAnyArgs { get; set; }
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax invocationExpr)
        {
            try
            {
                _currentInvocationContext.Enter();
                var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
                if (memberAccessExpr != null)
                {
                    var originalSymbolInfo = _originalSemantics.GetSymbolInfo(memberAccessExpr);

                    invocationExpr = base.VisitInvocationExpression(invocationExpr) as InvocationExpressionSyntax;
                    memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;

                    var originalMemberSymbol = originalSymbolInfo.Symbol as IMethodSymbol ?? originalSymbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
                    if (originalMemberSymbol != null)
                    {
                        if (RhinoRecognizer.TestReturnMethod(originalMemberSymbol))
                        {
                            if (_currentInvocationContext.Data.UseAnyArgs)
                                return invocationExpr.WithExpression(UseReturnsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseReturns(memberAccessExpr));
                        }
                        if (RhinoRecognizer.TestExpectMethod(originalMemberSymbol))
                        {
                            return DropExpectCall(memberAccessExpr);
                        }
                        if (RhinoRecognizer.TestGenerateMockMethod(originalMemberSymbol) || RhinoRecognizer.TestGenerateStubMethod(originalMemberSymbol))
                        {
                            return UseSubstituteFor(memberAccessExpr);
                        }
                        if (RhinoRecognizer.TestThrowMethod(originalMemberSymbol))
                        {
                            UseExceptionExtensions = true;
                            return invocationExpr.WithExpression(UseThrows(memberAccessExpr));
                        }
                        if (RhinoRecognizer.TestIgnoreArgumentsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax innerMemberExpression
                            && innerMemberExpression.Expression is InvocationExpressionSyntax innerInvocationExpression)
                        {
                            _currentInvocationContext.Data.UseAnyArgs = true;
                            return innerInvocationExpression;
                        }
                    }
                }
                return invocationExpr;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                _currentInvocationContext.Exit();
            }
        }

        public override SyntaxNode VisitArgument(ArgumentSyntax node)
        {
            var argumentExpr = node.Expression as MemberAccessExpressionSyntax;
            if (argumentExpr != null)
            {
                var argumentSymbol = _originalSemantics.GetSymbolInfo(argumentExpr).Symbol as IPropertySymbol;
                if (argumentSymbol != null)
                {
                    if (RhinoRecognizer.TestAnythingProperty(argumentSymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.TestIsArgProperty(innerSymbol))
                        {
                            return UseArgsAny(node);
                        }
                    }
                }
            }
            return base.VisitArgument(node);
        }

        private SyntaxNode DropExpectCall(SyntaxNode parentNode)
        {
            var mockedObjectIdentifier = (parentNode as MemberAccessExpressionSyntax).Expression as IdentifierNameSyntax;

            var expectInvocationExpression = parentNode.Parent as InvocationExpressionSyntax;
            if (expectInvocationExpression == null)
                return parentNode.Parent;
            var argumentLambda = expectInvocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression as LambdaExpressionSyntax;
            var mockMethodInvocation = argumentLambda.Body as InvocationExpressionSyntax;
            if (!(mockMethodInvocation?.Expression is MemberAccessExpressionSyntax mockedMethod))
                return parentNode.Parent;

            var invocation = SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                mockedObjectIdentifier, mockedMethod.Name), mockMethodInvocation.ArgumentList);

            return invocation;
        }

        private ExpressionSyntax UseReturns(ExpressionSyntax parentNode)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.WithName(SyntaxFactory.IdentifierName("Returns"));
            }
            return parentNode;
        }

        private ExpressionSyntax UseReturnsForAnyArgs(ExpressionSyntax parentNode)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.WithName(SyntaxFactory.IdentifierName("ReturnsForAnyArgs"));
            }
            return parentNode;
        }

        private ExpressionSyntax UseThrows(ExpressionSyntax parentNode)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.WithName(SyntaxFactory.IdentifierName("Throws"));
            }
            return parentNode;
        }

        private SyntaxNode UseSubstituteFor(SyntaxNode parentNode)
        {
            if (parentNode is MemberAccessExpressionSyntax memberAccess)
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
            }
            return parentNode;
        }

        public SyntaxNode UseArgsAny(ArgumentSyntax argument)
        {
            if (argument.Expression is MemberAccessExpressionSyntax anyProperty)
            {
                if (anyProperty.Name.ToString() == "Anything" && anyProperty.Expression is MemberAccessExpressionSyntax isProperty)
                {
                    if (isProperty.Expression is GenericNameSyntax argGenericArgument)
                    {
                        var typeArguments = argGenericArgument.TypeArgumentList;
                        var newArg = SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("NSubstitute"),
                            SyntaxFactory.IdentifierName("Arg")),
                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("Any"), typeArguments))));
                        return newArg;
                    }
                }
            }
            return argument;
        }

    }
}
