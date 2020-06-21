using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NXunitConverterAnalyzer.Data
{
    public class AssertThatData
    {
        public List<ArgumentSyntax> Arguments { get; set; } = new List<ArgumentSyntax>();

        public string AssertMethod { get; set; }

        public string AssertMethodTypeArgument { get; set; }

        public MemberAccessExpressionSyntax InvocationMember { get; set; }
    }
}
