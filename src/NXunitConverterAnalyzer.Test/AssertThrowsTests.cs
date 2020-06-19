using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertThrowsTests
    {
        [TestMethod]
        public async Task AssertThrowsReplacedAssertThrows()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThrows()
        {
            Assert.Throws<Exception>(() => new Exception());
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestAssertThrows()
        {
            Assert.Throws<Exception>(() => new Exception());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task AssertDoesNotThrowReplacedInnerLambda()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNotThrows()
        {
            Assert.DoesNotThrow(() => Console.WriteLine(""hello""));
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestAssertNotThrows()
        {
            Console.WriteLine(""hello"");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertNotThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task AssertDoesNotThrowAnonymusMethodReplacedInnerLambda()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertNotThrows()
        {
            Assert.DoesNotThrow(() =>
            {
                Console.WriteLine(""hello"");
                Console.WriteLine(""world"");
            });
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestAssertNotThrows()
        {
            Console.WriteLine(""hello"");
            Console.WriteLine(""world"");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertNotThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }


    }
}
