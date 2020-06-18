using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace NXunitConverterAnalyzer
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
        }
    }
}
