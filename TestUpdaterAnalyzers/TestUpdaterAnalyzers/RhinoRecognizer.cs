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

        public static bool TestIsArgProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Is", "Arg");

        public static bool TestAnythingProperty(IPropertySymbol propertySymbol) =>
            TestSymbol(propertySymbol, "Anything", "IsArg");

        public static bool TestGenerateStubMethod(IMethodSymbol memberSymbol) =>
          TestSymbol(memberSymbol, "GenerateStub", "MockRepository");

        public static bool TestThrowMethod(IMethodSymbol memberSymbol) =>
            TestSymbol(memberSymbol, "Throw", "IMethodOptions");

        public static bool TestSymbol(ISymbol symbolsType, string name, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }

    }
}
