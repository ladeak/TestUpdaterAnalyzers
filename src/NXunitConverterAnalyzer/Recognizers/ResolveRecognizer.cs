using Microsoft.CodeAnalysis;

namespace NXunitConverterAnalyzer.Recognizers
{
    public static class ResolveRecognizer
    {
        public static bool ResolveConstraint(ITypeSymbol symbol) => IsSymbol(symbol, "IResolveConstraint", null);

        public static bool TypeOfMethod(IMethodSymbol symbol) => IsSymbol(symbol, "TypeOf", "Is") && symbol.IsGenericMethod;

        public static bool ActualValueDelegate(ISymbol symbol) => IsSymbol(symbol, "ActualValueDelegate", null);

        public static bool TestDelegate(ISymbol symbol) => IsSymbol(symbol, "TestDelegate", null);

        public static bool ThrowsTypeOf(ISymbol symbol) => IsSymbol(symbol, "TypeOf", "Throws");

        public static bool NotProperty(ISymbol symbol) => IsSymbol(symbol, "Not", "Is");

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType != null
                && symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType?.Name == type;
        }
    }
}
