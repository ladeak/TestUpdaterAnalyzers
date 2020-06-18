using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertNullTests
    {
        [TestMethod]
        public async Task AssertIsNullReplacedAssertNull()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNull()
        {
            Assert.IsNull(null);
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
        public void TestAssertNull()
        {
            Assert.Null(null);
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertNull");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task AssertNullReplacedAssertNull()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNull()
        {
            Assert.Null(null);
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
        public void TestAssertNull()
        {
            Assert.Null(null);
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertNull");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task AssertIsNotNullReplacedAssertNotNull()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNull()
        {
            Assert.IsNotNull(new object());
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
        public void TestAssertNull()
        {
            Assert.NotNull(new object());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertNull");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task AssertNotNullReplacedAssertNotNull()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNull()
        {
            Assert.NotNull(new object());
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
        public void TestAssertNull()
        {
            Assert.NotNull(new object());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertNull");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

    }
}
