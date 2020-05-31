using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ConvertNxUnitAnalyzer
{
    public class XunitRewriter : CSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        private Document _originalDocument;
        private SyntaxWalkContext<MethodDeclarationData, MethodDeclarationSyntax> _methodDeclarationContext;

        public XunitRewriter()
        {
            _methodDeclarationContext = new SyntaxWalkContext<MethodDeclarationData, MethodDeclarationSyntax>(InitializeMethodDeclarationData);
        }

        public async Task<Document> UpdateToXUnitAsync(Document document, SemanticModel semanticModel, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _originalDocument = document;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newRoot = Visit(root);
            return document.WithSyntaxRoot(newRoot);
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node).Symbol;
            var newAttribute = base.VisitAttribute(node) as AttributeSyntax;

            if (NUnitRecognizer.IsTestAttribute(symbolInfo))
            {
                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact"));
            }
            if (NUnitRecognizer.IsTestCaseAttribute(symbolInfo))
            {
                return newAttribute.WithName(SyntaxFactory.IdentifierName("InlineData"));
            }
            return newAttribute;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            using (_methodDeclarationContext.Enter(node))
            {
                var newDeclaration = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;

                if (_methodDeclarationContext.Current.HasTestCase)
                {
                    var theoryAttribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Theory"))));
                    newDeclaration = newDeclaration.WithAttributeLists(newDeclaration.AttributeLists.Insert(0, theoryAttribute));
                }
                return newDeclaration;
            }
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node.Name).Symbol;
            var inner = base.VisitUsingDirective(node);

            if (NUnitRecognizer.IsNUnitUsingDirective(symbolInfo))
            {
                return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("Xunit"));
            }
            return inner;
        }

        private MethodDeclarationData InitializeMethodDeclarationData(MethodDeclarationSyntax node)
        {
            var initializer = new MethodDeclarationWalker(_semanticModel);
            return initializer.GetMethodDeclarationData(node);
        }
    }
}
