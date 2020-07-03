using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertTrueFalseTests
    {
        [TestMethod]
        public async Task IsTrue()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertTrue()
        {
            Assert.IsTrue(true);
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
        public void TestAssertTrue()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertTrue");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task True()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertTrue()
        {
            Assert.True(true);
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
        public void TestAssertTrue()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertTrue");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsFalse()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertFalse()
        {
            Assert.IsFalse(false);
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
        public void TestAssertFalse()
        {
            Assert.False(false);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertFalse");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task False()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertFalse()
        {
            Assert.False(false);
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
        public void TestAssertFalse()
        {
            Assert.False(false);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertFalse");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsTrueWithMessage()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertTrue()
        {
            Assert.IsTrue(true, ""should be ok"", ""param"");
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
        public void TestAssertTrue()
        {
            Assert.True(true, ""should be ok"");
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertTrue");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task TrueWithMessage()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertTrue()
        {
            Assert.True(true, ""should be ok"", ""param"");
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
        public void TestAssertTrue()
        {
            Assert.True(true, ""should be ok"");
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertTrue");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsFalseWithMessage()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertFalse()
        {
            Assert.IsFalse(false, ""should be ok"");
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
        public void TestAssertFalse()
        {
            Assert.False(false, ""should be ok"");
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertFalse");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task FalseWithMessage()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertFalse()
        {
            Assert.False(false, ""should be ok"", ""param"");
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
        public void TestAssertFalse()
        {
            Assert.False(false, ""should be ok"");
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertFalse");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }
}
