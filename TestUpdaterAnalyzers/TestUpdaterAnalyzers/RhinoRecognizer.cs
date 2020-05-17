using Microsoft.CodeAnalysis;

namespace TestUpdaterAnalyzers
{
    public static class RhinoRecognizer
    {
        public static bool IsReturnMethod(IMethodSymbol memberSymbol) =>
               IsSymbol(memberSymbol, "Return", "IMethodOptions");

        public static bool IsGenerateMockMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "GenerateMock", "MockRepository");

        public static bool IsExpectMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "Expect", "RhinoMocksExtensions");

        public static bool IsStubMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "Stub", "RhinoMocksExtensions");

        public static bool IsAssertWasCalledMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "AssertWasCalled", "RhinoMocksExtensions");

        public static bool IsAssertWasNotCalledMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "AssertWasNotCalled", "RhinoMocksExtensions");

        public static bool IsIsArgProperty(IPropertySymbol propertySymbol) =>
            IsSymbol(propertySymbol, "Is", "Arg");

        public static bool IsOutArgMethod(IMethodSymbol propertySymbol) =>
            IsSymbol(propertySymbol, "Out", "Arg");

        public static bool IsAnythingProperty(IPropertySymbol propertySymbol) =>
            IsSymbol(propertySymbol, "Anything", "IsArg");

        public static bool IsGenerateStubMethod(IMethodSymbol memberSymbol) =>
          IsSymbol(memberSymbol, "GenerateStub", "MockRepository");

        public static bool IsThrowMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "Throw", "IMethodOptions");

        public static bool IsIgnoreArgumentsMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "IgnoreArguments", "IMethodOptions");

        public static bool IsRepeatProperty(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "Repeat", "IMethodOptions");

        public static bool IsOutRefProperty(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "OutRef", "IMethodOptions");

        public static bool IsPropertyBehavior(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "PropertyBehavior", "IMethodOptions");

        public static bool IsVerifyAllExpectationsMethod(IMethodSymbol memberSymbol) =>
            IsSymbol(memberSymbol, "VerifyAllExpectations", "RhinoMocksExtensions");

        public static bool IsDummyField(IFieldSymbol fieldSymbol) =>
            IsSymbol(fieldSymbol, "Dummy", "OutRefArgDummy");

        public static bool IsAnyRepeatOptionsMethod(IMethodSymbol memberSymbol) =>
            IsAnySymbol(memberSymbol, "IRepeat");

        public static bool IsSymbol(ISymbol symbolsType, string name, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.Name == name
                && symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }

        public static bool IsAnySymbol(ISymbol symbolsType, string type, string assembly = "Rhino.Mocks")
        {
            return symbolsType.OriginalDefinition.ContainingAssembly.MetadataName == assembly
                && symbolsType.OriginalDefinition.ContainingType.Name == type;
        }
    }
}
