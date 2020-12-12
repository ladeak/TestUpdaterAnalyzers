using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NXunitConverterAnalyzer.Data;
using NXunitConverterAnalyzer.Recognizers;
using System;
using System.Linq;

namespace NXunitConverterAnalyzer.Walkers
{
    public class MethodDeclarationWalker : CSharpSyntaxWalker
    {
        private SemanticModel _semanticModel;
        private SyntaxWalkContext<MethodDeclarationData> _methodDeclarationContext;

        public MethodDeclarationWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            _methodDeclarationContext = new SyntaxWalkContext<MethodDeclarationData>();
        }

        public MethodDeclarationData GetMethodDeclarationData(MethodDeclarationSyntax node)
        {
            using (_methodDeclarationContext.Enter())
            {
                Visit(node);
                return _methodDeclarationContext.Current;
            }
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            var symbolInfo = _semanticModel.GetSymbolInfo(node).Symbol;
            if (AttributesRecognizer.IsTestCaseAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasTestCase = true;
            }
            if (AttributesRecognizer.IsTestAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasTestAttribute = true;
            }
            if (AttributesRecognizer.IsTestCaseSourceAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasTestCaseSourceAttribute = true;
            }
            if (AttributesRecognizer.IsSetUpAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasSetUp = true;
                return;
            }
            if (AttributesRecognizer.IsTearDownAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasTearDown = true;
                return;
            }
            if (AttributesRecognizer.IsOneTimeSetUpAttribute(symbolInfo))
            {
                _methodDeclarationContext.Current.HasOneTimeSetUp = true;
                return;
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            base.VisitInvocationExpression(node);
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (AssertRecognizer.DoesNotThrowMethod(symbol))
                GetInnerLambda(node, node.ArgumentList.Arguments.First().Expression);

            if (AssertRecognizer.DoesNotThrowAsyncMethod(symbol))
                GetInnerLambda(node, node.ArgumentList.Arguments.First().Expression);

        }

        private void GetInnerLambda(InvocationExpressionSyntax toBeReplaced, ExpressionSyntax expression)
        {
            if (expression is ParenthesizedLambdaExpressionSyntax lambda && lambda.Block != null)
            {
                var leadingTrivia = toBeReplaced.GetLeadingTrivia();
                _methodDeclarationContext.Current.BlockReplace
                    .Add(toBeReplaced, lambda.Block.Statements.Select(x => x.WithLeadingTrivia(leadingTrivia)).ToList());
            }
        }
    }
}
