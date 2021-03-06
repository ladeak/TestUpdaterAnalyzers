﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace NXunitConverterAnalyzer.Test
{
    public static class VerifyCodeFix
    {
        public static async Task VerifyFixAsync(string source, string fixtest, DiagnosticResult expected)
        {
            var fixTester = new CSharpCodeFixTest<NXunitConverterAnalyzer, NXunitConverterFixProvider, MSTestVerifier>();
            fixTester.ReferenceAssemblies = fixTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            fixTester.ReferenceAssemblies = fixTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("xunit", "2.4.1")));
            fixTester.TestCode = source;
            fixTester.FixedCode = fixtest;
            fixTester.TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck;
            fixTester.ExpectedDiagnostics.Add(expected);
            await fixTester.RunAsync();
        }

        public static async Task VerifyAnalyzerAsync(string source, DiagnosticResult expected)
        {
            var analysisTester = new CSharpCodeFixTest<NXunitConverterAnalyzer, NXunitConverterFixProvider, MSTestVerifier>();
            analysisTester.ReferenceAssemblies = analysisTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            analysisTester.TestCode = source;
            analysisTester.ExpectedDiagnostics.Add(expected);
            analysisTester.TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck;
            await analysisTester.RunAsync();
        }

        public static async Task VerifyAnalyzerAsync(string source)
        {
            var analysisTester = new CSharpCodeFixTest<NXunitConverterAnalyzer, NXunitConverterFixProvider, MSTestVerifier>();
            analysisTester.ReferenceAssemblies = analysisTester.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            analysisTester.TestCode = source;
            analysisTester.TestBehaviors = TestBehaviors.SkipGeneratedCodeCheck;
            await analysisTester.RunAsync();
        }
    }
}
