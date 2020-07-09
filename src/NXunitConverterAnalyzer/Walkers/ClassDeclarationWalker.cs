using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NXunitConverterAnalyzer.Data;
using NXunitConverterAnalyzer.Recognizers;
using System;
using System.Linq;

namespace NXunitConverterAnalyzer.Walkers
{
    public class ClassDeclarationWalker : CSharpSyntaxWalker
    {
        private SemanticModel _semanticModel;
        private SyntaxWalkContext<ClassDeclarationData> _classDeclarationContext;

        public ClassDeclarationWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            _classDeclarationContext = new SyntaxWalkContext<ClassDeclarationData>();
        }

        public ClassDeclarationData GetClassDeclarationData(ClassDeclarationSyntax node)
        {
            using (_classDeclarationContext.Enter())
            {
                Visit(node);
                return _classDeclarationContext.Current;
            }
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            var symbolInfo = _semanticModel.GetSymbolInfo(node).Symbol;
            if (AttributesRecognizer.IsTestCaseSourceAttribute(symbolInfo))
            {
                foreach (var arg in node.ArgumentList.Arguments.Select(x => x.Expression))
                {
                    if (arg is InvocationExpressionSyntax invocationExp
                        && invocationExp.Expression is IdentifierNameSyntax identifierName
                        && identifierName.Identifier.ValueText == "nameof")
                    {
                        var member = invocationExp.ArgumentList.Arguments.First().Expression as IdentifierNameSyntax;
                        _classDeclarationContext.Current.TestCaseSources.Add(member.Identifier);
                    }
                    if (arg is LiteralExpressionSyntax literalExp
                        && literalExp.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        _classDeclarationContext.Current.TestCaseSources.Add(literalExp.Token);
                    }
                }
            }
            if (AttributesRecognizer.IsTearDownAttribute(symbolInfo))
            {
                _classDeclarationContext.Current.HasTearDown = true;
            }
        }
    }
}
