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

                if (isResolveConstraint is null)
                    return null;

                if (isFunc)
                    _assertThatContext.Current.Arguments.Add(SyntaxFactory.Argument(
                        WrapInFunc(invocationExpression.ArgumentList.Arguments.First().Expression, symbol.TypeArguments.Select(x => x.Name))));
                else
                    _assertThatContext.Current.Arguments.Add(invocationExpression.ArgumentList.Arguments.First());

                var isResolveIndex = symbol.Parameters.IndexOf(isResolveConstraint);
                var contraintExpression = invocationExpression.ArgumentList.Arguments[isResolveIndex].Expression;
                var isConstraintSymbol = _semanticModel.GetSymbolInfo(invocationExpression.ArgumentList.Arguments[isResolveIndex].Expression).Symbol as ISymbol;

                _assertThatContext.Current.AssertMethod = Map(isConstraintSymbol.Name);

                if (contraintExpression is InvocationExpressionSyntax isInvocationExpr && isInvocationExpr.ArgumentList.Arguments.Any())
                    _assertThatContext.Current.Arguments.Add(isInvocationExpr.ArgumentList.Arguments.First());

                if (isConstraintSymbol is IMethodSymbol isContstrainedMethodSymbol)
                    _assertThatContext.Current.AssertMethodTypeArgument = isContstrainedMethodSymbol.TypeArguments.Select(x => x.Name).FirstOrDefault();

                return _assertThatContext.Current;
            }
        }

        public string Map(string name)
        {
            return name switch
            {
                "EqualTo" => "Equal",
                "True" => "True",
                "TypeOf" => "IsType",
                _ => string.Empty
            };
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

    }
}
