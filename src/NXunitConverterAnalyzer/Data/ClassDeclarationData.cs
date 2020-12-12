using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace NXunitConverterAnalyzer.Data
{
    public class ClassDeclarationData
    {
        public List<SyntaxToken> TestCaseSources { get; } = new List<SyntaxToken>();

        public bool HasTearDown { get; set; }

        public bool HasOneTimeSetup { get; set; }

        public List<SyntaxNode> OneTimeDeclarations { get; } = new List<SyntaxNode>();

        public INamedTypeSymbol ClassSymbol { get; set; }

        public List<ISymbol> SymbolsMoved { get; } = new List<ISymbol>();
    }
}