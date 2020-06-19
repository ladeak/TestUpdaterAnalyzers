using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NXunitConverterAnalyzer.Data
{
    public class MethodDeclarationData
    {
        public bool HasTestCase { get; set; }

        public bool HasTestAttribute { get; set; }

        public bool HasTestCaseSourceAttribute { get; set; }

        public Dictionary<InvocationExpressionSyntax, List<StatementSyntax>> BlockReplace { get; set; } = new Dictionary<InvocationExpressionSyntax, List<StatementSyntax>>();

    }
}