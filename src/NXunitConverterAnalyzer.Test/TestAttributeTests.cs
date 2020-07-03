using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class TestAttributeTests
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
            var source = 
@"using NUnit.Framework;

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

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("Test1");
            await VerifyCodeFix.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task TestAttributeReplacedWithFact()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void Test1()
        {
        }
    }
}";

            var fixtest =
@"using Xunit;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void Test1()
        {
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("Test1");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }
}
