using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace ConvertNxUnitAnalyzer.Test
{
    public static class VerifyCodeFix
    {
        public static async Task VerifyFixAsync(string source, string fixtest, DiagnosticResult expected)
        {
            var fixTester = new CSharpCodeFixTest<ConvertNxUnitAnalyzer, ConvertNxUnitCodeFixProvider, MSTestVerifier>();
            fixTester.ReferenceAssemblies = fixTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            fixTester.ReferenceAssemblies = fixTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("xunit", "2.4.1")));
            fixTester.TestCode = source;
            fixTester.FixedCode = fixtest;
            fixTester.ExpectedDiagnostics.Add(expected);
            await fixTester.RunAsync();
        }

        public static async Task VerifyAnalyzerAsync(string source, DiagnosticResult expected)
        {
            var analysisTester = new CSharpCodeFixTest<ConvertNxUnitAnalyzer, ConvertNxUnitCodeFixProvider, MSTestVerifier>();
            analysisTester.ReferenceAssemblies = analysisTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            analysisTester.TestCode = source;
            analysisTester.ExpectedDiagnostics.Add(expected);
            await analysisTester.RunAsync();
        }
    }
}
