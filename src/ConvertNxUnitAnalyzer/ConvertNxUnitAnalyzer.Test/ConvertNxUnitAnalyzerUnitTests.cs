using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConvertNxUnitAnalyzer;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    ConvertNxUnitAnalyzer.ConvertNxUnitAnalyzer,
    ConvertNxUnitAnalyzer.ConvertNxUnitCodeFixProvider>;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace ConvertNxUnitAnalyzer.Test
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task EmptyDocument_NoDiagnostics()
        {
            var test = @"";

            await Verify.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task NUnitTestAttribute_DiagnosticWarning()
        {
            var source = @"
using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTests
    {
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(8, 9).WithArguments("Test1");

            var fixture = new CSharpCodeFixTest<ConvertNxUnitAnalyzer, ConvertNxUnitCodeFixProvider, MSTestVerifier>();
            fixture.ReferenceAssemblies = fixture.ReferenceAssemblies.AddPackages(ImmutableArray.Create(new PackageIdentity("nunit", "3.12.0")));
            fixture.TestCode = source;
            fixture.ExpectedDiagnostics.Add(expected);
            await fixture.RunAsync();
        }


        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTests
    {
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}";

            var fixtest = @"using Xunit;

namespace NUnitToXUnitTests
{
    public class xUnitTests
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(11, 15).WithArguments("TypeName");
            await Verify.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
