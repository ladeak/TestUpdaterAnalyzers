using Microsoft.CodeAnalysis;
using System.Linq;

namespace NXunitConverterAnalyzer
{
    public static class NUnitRecognizer
    {
        public static bool IsNUnitUsingDirective(ISymbol symbol) => IsNamespaceSymbol(symbol, "Framework");

        public static bool IsTestAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestAttribute");

        public static bool IsTestCaseAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseAttribute");

        public static bool IsTestCaseSourceAttribute(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseSourceAttribute");

        public static bool IsTestCaseData(ISymbol symbol) => IsNamespaceSymbol(symbol, "TestCaseData");

        public static bool IsTestCaseDataCtor(ISymbol symbol) => IsSymbol(symbol, ".ctor", "TestCaseData");

        public static bool IsAssertIsTrueMethod(ISymbol symbol) => IsSymbol(symbol, "IsTrue", "Assert");

        public static bool IsAssertIsFalseMethod(ISymbol symbol) => IsSymbol(symbol, "IsFalse", "Assert");

        public static bool IsAssertAreEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreEqual", "Assert");

        public static bool IsAssertAreNotEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotEqual", "Assert");

        public static bool IsAssertAreSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreSame", "Assert");

        public static bool IsAssertAreNotSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotSame", "Assert");

        public static bool IsAssertIsNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNull", "Assert");

        public static bool IsAssertIsNotNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotNull", "Assert");

        public static bool IsAssertIsEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsEmpty", "Assert");

        public static bool IsAssertIsNotEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotEmpty", "Assert");

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
