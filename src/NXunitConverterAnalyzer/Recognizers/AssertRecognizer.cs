using Microsoft.CodeAnalysis;

namespace NXunitConverterAnalyzer.Recognizers
{
    public static class AssertRecognizer
    {
        public static bool IsTrueMethod(ISymbol symbol) => IsSymbol(symbol, "IsTrue", "Assert");

        public static bool IsFalseMethod(ISymbol symbol) => IsSymbol(symbol, "IsFalse", "Assert");

        public static bool AreEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreEqual", "Assert");

        public static bool AreNotEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotEqual", "Assert");

        public static bool AreSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreSame", "Assert");

        public static bool AreNotSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotSame", "Assert");

        public static bool IsNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNull", "Assert");

        public static bool IsNotNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotNull", "Assert");

        public static bool IsEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsEmpty", "Assert");

        public static bool IsNotEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotEmpty", "Assert");

        public static bool ZeroMethod(ISymbol symbol) => IsSymbol(symbol, "Zero", "Assert");

        public static bool NotZeroMethod(ISymbol symbol) => IsSymbol(symbol, "NotZero", "Assert");

        public static bool PassMethod(ISymbol symbol) => IsSymbol(symbol, "Pass", "Assert");

        public static bool FailMethod(ISymbol symbol) => IsSymbol(symbol, "Fail", "Assert");

        public static bool ThrowsMethod(ISymbol symbol) => IsSymbol(symbol, "Throws", "Assert");

        public static bool DoesNotThrowMethod(ISymbol symbol) => IsSymbol(symbol, "DoesNotThrow", "Assert");

        public static bool ThrowsAsyncMethod(ISymbol symbol) => IsSymbol(symbol, "ThrowsAsync", "Assert");

        public static bool DoesNotThrowAsyncMethod(ISymbol symbol) => IsSymbol(symbol, "DoesNotThrowAsync", "Assert");

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType != null
                && symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }
    }
}
