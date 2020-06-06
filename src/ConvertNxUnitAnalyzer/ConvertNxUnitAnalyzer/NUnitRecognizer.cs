using Microsoft.CodeAnalysis;

namespace ConvertNxUnitAnalyzer
{
    public static class NUnitRecognizer
    {
        public static bool IsNUnitUsingDirective(ISymbol symbol) => IsNamespaceSymbol(symbol, "Framework");

        public static bool IsTestAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestAttribute");

        public static bool IsTestCaseAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseAttribute");

        public static bool IsTestCaseSourceAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseSourceAttribute");

        public static bool IsTestCaseData(ISymbol symbol) => IsNamespaceSymbol(symbol, "TestCaseData");

        public static bool IsTestCaseDataCtor(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseData");

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType != null 
                && symbolsType.Name == name
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
