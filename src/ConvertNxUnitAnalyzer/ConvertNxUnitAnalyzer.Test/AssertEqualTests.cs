using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.ConvertNxUnitCodeFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertEqualTests
    {
        [TestMethod]
        public async Task AssertAreEqualReplacedAssertEqual()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertAreEqual()
        {
            Assert.AreEqual(0, 0);
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
        public void TestAssertAreEqual()
        {
            Assert.Equal(0, 0);
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertAreEqual");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

    }
}
