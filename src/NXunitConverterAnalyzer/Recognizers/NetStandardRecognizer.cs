using Microsoft.CodeAnalysis;

namespace NXunitConverterAnalyzer.Recognizers
{
    public static class NetStandardRecognizer
    {
        public static bool IsIEnumerableParameter(ITypeSymbol symbol) => IsType(symbol, "IEnumerable", null);

        public static bool IsStringParameter(ITypeSymbol symbol) => IsType(symbol, "String", null);

        public static bool IsFuncParameter(ITypeSymbol symbol) => IsType(symbol, "Func", null);

        private static bool IsType(ITypeSymbol symbolsType, string name, string type, string assembly = "netstandard")
        {
            return symbolsType != null
                && symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType?.Name == type;
        }

    }
}
