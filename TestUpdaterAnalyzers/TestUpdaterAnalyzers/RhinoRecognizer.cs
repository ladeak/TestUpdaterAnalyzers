using Microsoft.CodeAnalysis;

namespace TestUpdaterAnalyzers
{
    public static class RhinoRecognizer
    {
        public static bool TestReturnMethod(IMethodSymbol memberSymbol) =>
               TestSymbol(memberSymbol, "Return", "IMethodOptions");

        public static bool TestGenerateMockMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "GenerateMock", "MockRepository");

        public static bool TestExpectMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Expect", "RhinoMocksExtensions");

        public static bool TestStubMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Stub", "RhinoMocksExtensions");

        public static bool TestIsArgProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Is", "Arg");

        public static bool TestAnythingProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Anything", "IsArg");

        public static bool TestGenerateStubMethod(IMethodSymbol memberSymbol) =>
          TestSymbol(memberSymbol, "GenerateStub", "MockRepository");

        public static bool TestThrowMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Throw", "IMethodOptions");

        public static bool TestIgnoreArgumentsMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "IgnoreArguments", "IMethodOptions");

        public static bool TestRepeatProperty(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Repeat", "IMethodOptions");

        public static bool TestOutRefProperty(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "OutRef", "IMethodOptions");

        public static bool TestAnyRepeatOptionsMethod(IMethodSymbol memberSymbol) =>
            TestAnySymbol(memberSymbol, "IRepeat");

        public static bool TestSymbol(ISymbol symbolsType, string name, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }

        public static bool TestAnySymbol(ISymbol symbolsType, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }

    }
}
