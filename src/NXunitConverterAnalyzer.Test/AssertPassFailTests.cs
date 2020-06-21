using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertPassFailTests
    {
        [TestMethod]
        public async Task Pass()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertPassFail()
        {
            Assert.Pass();
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
        public void TestAssertPassFail()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertPassFail");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task Fail()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertPassFail()
        {
            Assert.Fail(""msg"");
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
        public void TestAssertPassFail()
        {
            Assert.True(false, ""msg"");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertPassFail");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

    }
}
