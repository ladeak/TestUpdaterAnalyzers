using Microsoft.CodeAnalysis;

namespace ConvertNxUnitAnalyzer
{
    public static class NUnitRecognizer
    {
        public static bool IsNUnitUsingDirective(ISymbol symbol) => IsNamespaceSymbol(symbol, "Framework");

        public static bool IsTestAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestAttribute");

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }

        private static bool IsNamespaceSymbol(ISymbol symbolsType, string name, string assembly = "nunit.framework")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly;
        }
    }
}
