using Microsoft.CodeAnalysis;

namespace NXunitConverterAnalyzer.Recognizers
{
    public static class AssertRecognizer
    {
        public static bool IsTrueMethod(ISymbol symbol) => IsSymbol(symbol, "IsTrue", "Assert");

        public static bool IsFalseMethod(ISymbol symbol) => IsSymbol(symbol, "IsFalse", "Assert");

        public static bool TrueMethod(ISymbol symbol) => IsSymbol(symbol, "True", "Assert");

        public static bool FalseMethod(ISymbol symbol) => IsSymbol(symbol, "False", "Assert");

        public static bool AreEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreEqual", "Assert");

        public static bool AreNotEqualMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotEqual", "Assert");

        public static bool AreSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreSame", "Assert");

        public static bool AreNotSameMethod(ISymbol symbol) => IsSymbol(symbol, "AreNotSame", "Assert");

        public static bool IsNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNull", "Assert");

        public static bool IsNotNullMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotNull", "Assert");

        public static bool NullMethod(ISymbol symbol) => IsSymbol(symbol, "Null", "Assert");

        public static bool NotNullMethod(ISymbol symbol) => IsSymbol(symbol, "NotNull", "Assert");

        public static bool IsEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsEmpty", "Assert");

        public static bool IsNotEmptyMethod(ISymbol symbol) => IsSymbol(symbol, "IsNotEmpty", "Assert");

        public static bool ContainsMethod(ISymbol symbol) => IsSymbol(symbol, "Contains", "Assert");

        public static bool ZeroMethod(ISymbol symbol) => IsSymbol(symbol, "Zero", "Assert");

        public static bool NotZeroMethod(ISymbol symbol) => IsSymbol(symbol, "NotZero", "Assert");

        public static bool PassMethod(ISymbol symbol) => IsSymbol(symbol, "Pass", "Assert");

        public static bool FailMethod(ISymbol symbol) => IsSymbol(symbol, "Fail", "Assert");

        public static bool ThrowsMethod(IMethodSymbol symbol) => IsSymbol(symbol, "Throws", "Assert") && symbol.IsGenericMethod;

        public static bool DoesNotThrowMethod(ISymbol symbol) => IsSymbol(symbol, "DoesNotThrow", "Assert");

        public static bool ThrowsAsyncMethod(IMethodSymbol symbol) => IsSymbol(symbol, "ThrowsAsync", "Assert") && symbol.IsGenericMethod;

        public static bool DoesNotThrowAsyncMethod(ISymbol symbol) => IsSymbol(symbol, "DoesNotThrowAsync", "Assert");

        public static bool IsInstanceOfMethod(IMethodSymbol symbol) => IsSymbol(symbol, "IsInstanceOf", "Assert") && symbol.IsGenericMethod;

        public static bool IsNotInstanceOfMethod(IMethodSymbol symbol) => IsSymbol(symbol, "IsNotInstanceOf", "Assert") && symbol.IsGenericMethod;

        public static bool IsAssignableFromMethod(IMethodSymbol symbol) => IsSymbol(symbol, "IsAssignableFrom", "Assert") && symbol.IsGenericMethod;

        public static bool IsNotAssignableFromMethod(IMethodSymbol symbol) => IsSymbol(symbol, "IsNotAssignableFrom", "Assert") && symbol.IsGenericMethod;

        public static bool ThatNotGenericMethod(IMethodSymbol symbol) => IsSymbol(symbol, "That", "Assert") && !symbol.IsGenericMethod;

        public static bool ThatMethod(IMethodSymbol symbol) => IsSymbol(symbol, "That", "Assert") && symbol.IsGenericMethod;

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType != null
                && symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }
    }
}
