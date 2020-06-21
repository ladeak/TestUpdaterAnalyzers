using Microsoft.CodeAnalysis;

namespace NXunitConverterAnalyzer.Recognizers
{
    public static class ResolveRecognizer
    {
        public static bool ResolveConstraint(ITypeSymbol symbol) => IsSymbol(symbol, "IResolveConstraint", null);

        public static bool TypeOfMethod(IMethodSymbol symbol) => IsSymbol(symbol, "TypeOf", "Is") && symbol.IsGenericMethod;

        public static bool ActualValueDelegate(ISymbol symbol) => IsSymbol(symbol, "ActualValueDelegate", null);

        private static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "nunit.framework")
        {
            return symbolsType != null
                && symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType?.Name == type;
        }
    }
}
