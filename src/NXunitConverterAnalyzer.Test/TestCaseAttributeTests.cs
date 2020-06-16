using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.ConvertNxUnitCodeFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class TestCaseAttributeTests
    {
        [TestMethod]
        public async Task TestCaseAttribute_DiagnosticWarning()
        {
            var source = 
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTests
    {
        [TestCase(""value"")]
        [TestCase(""value1"")]
        public void TestCase(string value)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestCase");
            await VerifyCodeFix.VerifyAnalyzerAsync(source, expected);
        }


        [TestMethod]
        public async Task TestCaseAttributeReplacedWithInlineData()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [TestCase(""value"")]
        [TestCase(""value1"")]
        public void TestCase(string value)
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
        [Theory]
        [InlineData(""value"")]
        [InlineData(""value1"")]
        public void TestCase(string value)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestCase");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task TestAndTestCaseAttributeReplacedWithInlineData()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        [TestCase(""value"")]
        [TestCase(""value1"")]
        public void TestCase(string value)
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
        [Theory]
        [InlineData(""value"")]
        [InlineData(""value1"")]
        public void TestCase(string value)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestCase");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

    }
}
