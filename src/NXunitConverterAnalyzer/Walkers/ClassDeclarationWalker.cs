using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using NXunitConverterAnalyzer.Data;
using NXunitConverterAnalyzer.Recognizers;
using System;
using System.Linq;

namespace NXunitConverterAnalyzer.Walkers
{
    public class ClassDeclarationWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly Document _originalDocument;
        private SyntaxWalkContext<ClassDeclarationData> _classDeclarationContext;
        private bool _collectIndentifiers = false;

        public ClassDeclarationWalker(SemanticModel semanticModel, Document originalDocument)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            _originalDocument = originalDocument ?? throw new ArgumentNullException(nameof(originalDocument));
            _classDeclarationContext = new SyntaxWalkContext<ClassDeclarationData>();
        }

        public ClassDeclarationData GetClassDeclarationData(ClassDeclarationSyntax node)
        {
            using (_classDeclarationContext.Enter())
            {
                _classDeclarationContext.Current.ClassSymbol = _semanticModel.GetDeclaredSymbol(node);
                Visit(node);
                return _classDeclarationContext.Current;
            }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            base.VisitMethodDeclaration(node);
            _collectIndentifiers = false;
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
            if (AttributesRecognizer.IsOneTimeSetUpAttribute(symbolInfo))
            {
                _classDeclarationContext.Current.HasOneTimeSetup = true;
            }
            if (AttributesRecognizer.IsOneTimeSetUpAttribute(symbolInfo))
            {
                _collectIndentifiers = true;
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (!_collectIndentifiers)
                return;
            base.VisitIdentifierName(node);
            var declarations = SymbolFinder.FindDeclarationsAsync(_originalDocument.Project, node.Identifier.ValueText,
                false, SymbolFilter.Member).Result;

            foreach (var declaration in declarations)
            {
                if (SymbolEqualityComparer.Default.Equals(declaration.ContainingType, _classDeclarationContext.Current.ClassSymbol))
                {
                    var location = declaration.Locations.First();
                    var declarationSyntax = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
                    _classDeclarationContext.Current.OneTimeDeclarations.Add(declarationSyntax);
                    
                    var identifierSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
                    _classDeclarationContext.Current.SymbolsMoved.Add(identifierSymbol);

                }
            }
        }
    }
}
