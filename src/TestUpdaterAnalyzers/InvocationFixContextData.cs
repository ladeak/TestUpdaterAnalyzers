using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace TestUpdaterAnalyzers
{
    public class InvocationFixContextData
    {
        public bool UseAnyArgs { get; set; }

        public List<ExpressionSyntax> OutRefArguments { get; } = new List<ExpressionSyntax>();

        public List<ArgumentSyntax> OriginalArguments { get; } = new List<ArgumentSyntax>();

        public SimpleLambdaExpressionSyntax WhenCalledLambda { get; set; }

        public bool HasReturn { get; set; }

        public bool HasThrow { get; set; }

        public KeyValuePair<string, ExpressionSyntax> ExpectCallForAssertion { get; set; }

        public InvocationExpressionSyntax IsRemovable { get; set; }

        public bool UseExceptionExtensions { get; set; }
    }
}
