using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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
                            invocationExpr = invocationExpr.WithArgumentList(ReWriteOutRefArguments(invocationExpr));
                            if (_currentInvocationContext.Data.UseAnyArgs)
                                return invocationExpr.WithExpression(UseReturnsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseReturns(memberAccessExpr));

                        }
                        if (RhinoRecognizer.TestExpectMethod(originalMemberSymbol) || RhinoRecognizer.TestStubMethod(originalMemberSymbol))
                        {
                            return DropExpectOrStubCall(memberAccessExpr);
                        }
                        if (RhinoRecognizer.TestGenerateMockMethod(originalMemberSymbol) || RhinoRecognizer.TestGenerateStubMethod(originalMemberSymbol))
                        {
                            return UseSubstituteFor(memberAccessExpr);
                        }
                        if (RhinoRecognizer.TestThrowMethod(originalMemberSymbol))
                        {
                            UseExceptionExtensions = true;
                            if (_currentInvocationContext.Data.UseAnyArgs)
                                return invocationExpr.WithExpression(UseThrowsForAnyArgs(memberAccessExpr));
                            else
                                return invocationExpr.WithExpression(UseThrows(memberAccessExpr));
                        }
                        if (RhinoRecognizer.TestIgnoreArgumentsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax ignoreArgumentsMemberExpression
                            && ignoreArgumentsMemberExpression.Expression is InvocationExpressionSyntax innerIgnoreArgumentsInvocationExpression)
                        {
                            _currentInvocationContext.Data.UseAnyArgs = true;
                            return innerIgnoreArgumentsInvocationExpression;
                        }
                        if (RhinoRecognizer.TestAnyRepeatOptionsMethod(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax repeatOptionMemberAccess
                            && repeatOptionMemberAccess.Expression is MemberAccessExpressionSyntax repeatMemberAccess
                            && repeatMemberAccess.Expression != null)
                        {
                            return repeatMemberAccess.Expression;
                        }
                        if (RhinoRecognizer.TestOutRefProperty(originalMemberSymbol)
                            && invocationExpr.Expression is MemberAccessExpressionSyntax outRefMemberExpression
                            && outRefMemberExpression.Expression is InvocationExpressionSyntax outRefInnerInvocationExpression)
                        {
                            _currentInvocationContext.Data.OutRefArguments.AddRange(invocationExpr.ArgumentList.Arguments.Select(x => x.Expression));
                            return outRefInnerInvocationExpression;
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
                var argumentSymbol = _originalSemantics.GetSymbolInfo(argumentExpr).Symbol;
                if (argumentSymbol is IPropertySymbol propertySymbol)
                {
                    if (RhinoRecognizer.TestAnythingProperty(propertySymbol))
                    {
                        var innerSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IPropertySymbol;
                        if (RhinoRecognizer.TestIsArgProperty(innerSymbol))
                        {
                            return UseArgsAny(node);
                        }
                    }
                }

                if (node.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) && argumentSymbol is IFieldSymbol fieldSymbol)
                {
                    if (RhinoRecognizer.TestDummyField(fieldSymbol))
                    {
                        var outMethodSymbol = _originalSemantics.GetSymbolInfo(argumentExpr.Expression).Symbol as IMethodSymbol;
                        if (RhinoRecognizer.TestOutArgMethod(outMethodSymbol) && argumentExpr.Expression is InvocationExpressionSyntax outMethodInvocation)
                        {
                            _currentInvocationContext.Data.OutRefArguments.Add(outMethodInvocation.ArgumentList.Arguments.First().Expression);
                            return UseArgsAny(node, true);
                        }
                    }
                }
            }
            return base.VisitArgument(node);
        }



        private SyntaxNode DropExpectOrStubCall(SyntaxNode parentNode)
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

            _currentInvocationContext.Data.OriginalArguments.AddRange(mockMethodInvocation.ArgumentList.Arguments);
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
            if (!_currentInvocationContext.Data.OutRefArguments.Any()
                || !_currentInvocationContext.Data.OriginalArguments.Any())
                return invocation.ArgumentList;

            var paramToken = SyntaxFactory.ParseToken("c");

            int currentOutArgumentIndex = 0;
            List<StatementSyntax> statements = new List<StatementSyntax>();
            foreach (var outArg in _currentInvocationContext.Data.OutRefArguments)
            {
                currentOutArgumentIndex = _currentInvocationContext.Data.OriginalArguments.FindIndex(currentOutArgumentIndex, x => x.RefKindKeyword.IsKind(SyntaxKind.OutKeyword));
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
