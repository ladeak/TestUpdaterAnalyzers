using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TestUpdaterAnalyzers
{
    public class InvocationFixContextData
    {
        public bool UseAnyArgs { get; set; }

        public List<ExpressionSyntax> OutRefArguments { get; } = new List<ExpressionSyntax>();

        public List<ArgumentSyntax> OriginalArguments { get; } = new List<ArgumentSyntax>();
    }
}
