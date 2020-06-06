using System;
using System.Linq;
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
        private SyntaxWalkContext<ClassDeclarationData, ClassDeclarationSyntax> _classDeclarationContext;

        public XunitRewriter()
        {
            _methodDeclarationContext = new SyntaxWalkContext<MethodDeclarationData, MethodDeclarationSyntax>(InitializeMethodDeclarationData);
            _classDeclarationContext = new SyntaxWalkContext<ClassDeclarationData, ClassDeclarationSyntax>(InitializeClassDeclarationData);
        }

        public async Task<Document> UpdateToXUnitAsync(Document document, SemanticModel semanticModel, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            _semanticModel = semanticModel;
            _originalDocument = document;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var newRoot = Visit(root);
            return document.WithSyntaxRoot(newRoot);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            using (_classDeclarationContext.Enter(node))
                return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node).Symbol;
            var newAttribute = base.VisitAttribute(node) as AttributeSyntax;

            if (NUnitRecognizer.IsTestAttribute(symbolInfo))
            {
                var newAttributeName = (_methodDeclarationContext.Current.HasTestCase || _methodDeclarationContext.Current.HasTestCaseSourceAttribute) ? "Theory" : "Fact";
                return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(newAttributeName));
            }
            if (NUnitRecognizer.IsTestCaseAttribute(symbolInfo))
            {
                return newAttribute.WithName(SyntaxFactory.IdentifierName("InlineData"));
            }
            if (NUnitRecognizer.IsTestCaseSourceAttribute(symbolInfo) && newAttribute.ArgumentList.Arguments.Count == 1)
            {
                return newAttribute.WithName(SyntaxFactory.IdentifierName("MemberData"));
            }
            return newAttribute;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            using (_methodDeclarationContext.Enter(node))
            {
                var newDeclaration = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;

                // Add Theory if no Test attribute to replace.
                if ((_methodDeclarationContext.Current.HasTestCase || _methodDeclarationContext.Current.HasTestCaseSourceAttribute)
                    && !_methodDeclarationContext.Current.HasTestAttribute)
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

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var newProperty = base.VisitPropertyDeclaration(node) as PropertyDeclarationSyntax;
            if (_classDeclarationContext.Current.TestCaseSources.Any(x => x.ValueText == newProperty.Identifier.ValueText))
            {
                if (newProperty.Type is GenericNameSyntax genericType
                    && genericType.TypeArgumentList.Arguments.Count == 1
                    && genericType.TypeArgumentList.Arguments.First() is IdentifierNameSyntax closedType)
                {
                    var genericSymbol = _semanticModel.GetSymbolInfo(closedType).Symbol;
                    if (!NUnitRecognizer.IsTestCaseData(genericSymbol))
                        return newProperty;

                    newProperty = newProperty.WithType(genericType.WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                          SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                 SyntaxFactory.ArrayType(
                                     SyntaxFactory.PredefinedType(
                                         SyntaxFactory.Token(SyntaxKind.ObjectKeyword)
                                     ), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                         SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))
                                 )
                             )
                          )
                        )
                    );
                }


            }


            return newProperty;
        }

        private MethodDeclarationData InitializeMethodDeclarationData(MethodDeclarationSyntax node)
        {
            var initializer = new MethodDeclarationWalker(_semanticModel);
            return initializer.GetMethodDeclarationData(node);
        }

        private ClassDeclarationData InitializeClassDeclarationData(ClassDeclarationSyntax node)
        {
            var initializer = new ClassDeclarationWalker(_semanticModel);
            return initializer.GetClassDeclarationData(node);
        }
    }
}
