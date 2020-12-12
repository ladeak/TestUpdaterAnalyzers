using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NXunitConverterAnalyzer.Data;
using NXunitConverterAnalyzer.Recognizers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NXunitConverterAnalyzer.Walkers
{

    public class AssertThatWalker : CSharpSyntaxWalker
    {
        private SemanticModel _semanticModel;
        private SyntaxWalkContext<AssertThatData> _assertThatContext;

        public AssertThatWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            _assertThatContext = new SyntaxWalkContext<AssertThatData>();
        }

        public AssertThatData GetAssertThatData(InvocationExpressionSyntax invocationExpression)
        {
            using (_assertThatContext.Enter())
            {
                Visit(invocationExpression);
                var symbol = _semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
                _assertThatContext.Current.InvocationMember = invocationExpression.Expression as MemberAccessExpressionSyntax;

                var isResolveConstraint = symbol.Parameters.FirstOrDefault(x => ResolveRecognizer.ResolveConstraint(x.Type));
                var isFunc = ResolveRecognizer.ActualValueDelegate(symbol.Parameters.First().Type);
                var isAction = ResolveRecognizer.TestDelegate(symbol.Parameters.First().Type);

                if (isResolveConstraint is null)
                    return null;

                if (isFunc)
                {
                    _assertThatContext.Current.Arguments.Add(SyntaxFactory.Argument(
                        WrapInFunc(invocationExpression.ArgumentList.Arguments.First().Expression, symbol.TypeArguments.Select(x => x.Name))));
                }
                else if (isAction)
                {
                    _assertThatContext.Current.Arguments.Add(SyntaxFactory.Argument(
                        WrapInAction(invocationExpression.ArgumentList.Arguments.First().Expression)));
                }
                else
                    _assertThatContext.Current.Arguments.Add(invocationExpression.ArgumentList.Arguments.First());

                var isResolveIndex = symbol.Parameters.IndexOf(isResolveConstraint);
                var constraintData = WalkConstraintExpression(invocationExpression.ArgumentList.Arguments[isResolveIndex].Expression);

                _assertThatContext.Current.AssertMethod = constraintData.ConstraintMode;
                _assertThatContext.Current.AssertMethodTypeArgument = constraintData.ConstraintGenericType;
                if (constraintData.ConstraintArgument != null)
                    _assertThatContext.Current.Arguments.Add(constraintData.ConstraintArgument);

                return _assertThatContext.Current;
            }
        }

        private ConstraintData WalkConstraintExpression(ExpressionSyntax contraintExpression)
        {
            ConstraintWalker walker = new ConstraintWalker();
            var contraintData = walker.GetConstraintData(contraintExpression);
            return contraintData;
        }


        private InvocationExpressionSyntax WrapInFunc(ExpressionSyntax expression, IEnumerable<string> genericTypes)
        {
            ExpressionSyntax wrappedFunc = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.GenericName(SyntaxFactory.Identifier("Func"),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(genericTypes.Select(x => SyntaxFactory.ParseTypeName(x))))),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(expression))), null);

            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                wrappedFunc, SyntaxFactory.IdentifierName("Invoke")));
        }

        private ExpressionSyntax WrapInAction(ExpressionSyntax expression)
        {
            ExpressionSyntax wrappedFunc = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("Action"),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(expression))), null);
            return wrappedFunc;
        }
    }
}
