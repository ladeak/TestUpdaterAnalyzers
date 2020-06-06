using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ConvertNxUnitAnalyzer
{
    public class ClassDeclarationData
    {
        public List<SyntaxToken> TestCaseSources { get; } = new List<SyntaxToken>();
    }
}