using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertIsInstanceOfTests
    {
        [TestMethod]
        public async Task IsInstanceOfTest()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertIsInstanceOf()
        {
            Assert.IsInstanceOf<Exception>(new ArgumentNullException(), ""some message"");
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
        public void TestAssertIsInstanceOf()
        {
            Assert.IsAssignableFrom<Exception>(new ArgumentNullException());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertIsInstanceOf");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsNotInstanceOfTest()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertIsNotInstanceOf()
        {
            Assert.IsNotInstanceOf<ArgumentNullException>(new Exception());
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
        public void TestAssertIsNotInstanceOf()
        {
            Assert.False(new Exception() is ArgumentNullException);
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertIsNotInstanceOf");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsAssignableFromTest()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertIsAssignableFrom()
        {
            Assert.IsAssignableFrom<ArgumentNullException>(new Exception(), ""some message"");
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
        public void TestAssertIsAssignableFrom()
        {
            Assert.True(new Exception().GetType().IsAssignableFrom(typeof(ArgumentNullException)));
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertIsAssignableFrom");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task IsNotAssignableFromTest()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertIsAssignableFrom()
        {
            Assert.IsNotAssignableFrom<ArgumentNullException>(new DivideByZeroException());
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
        public void TestAssertIsAssignableFrom()
        {
            Assert.False(new DivideByZeroException().GetType().IsAssignableFrom(typeof(ArgumentNullException)));
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertIsAssignableFrom");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

    }
}
