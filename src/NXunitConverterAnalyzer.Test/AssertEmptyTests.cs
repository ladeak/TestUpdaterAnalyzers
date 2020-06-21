using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertEmptyTests
    {
        [TestMethod]
        public async Task IsEmptyCollection()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertEmpty()
        {
            Assert.IsEmpty(new List<object>(), ""some message"", ""param"");
        }
    }
}";

            var fixtest =
@"using Xunit;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestAssertEmpty()
        {
            Assert.Empty(new List<object>());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertEmpty");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsEmptyString()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertEmpty()
        {
            Assert.IsEmpty("""");
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
        public void TestAssertEmpty()
        {
            Assert.Equal(string.Empty, """");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertEmpty");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsNotEmptyCollection()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNotEmpty()
        {
            Assert.IsNotEmpty(new List<object>());
        }
    }
}";

            var fixtest =
@"using Xunit;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestAssertNotEmpty()
        {
            Assert.NotEmpty(new List<object>());
        }
    }
}";


            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertNotEmpty");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsNotEmptyString()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNotEmpty()
        {
            Assert.IsNotEmpty("""", ""some message"", ""param"");
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
        public void TestAssertNotEmpty()
        {
            Assert.NotEqual(string.Empty, """");
        }
    }
}";


            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertNotEmpty");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }
}
