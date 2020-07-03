using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class AssertThatTests
    {
        [TestMethod]
        public async Task ThatIsEqualTo()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(5, Is.EqualTo(5));
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
        public void TestAssertThat()
        {
            Assert.Equal(5, 5);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsEqualToDelegete()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => 5, Is.EqualTo(5));
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
        public void TestAssertThat()
        {
            Assert.Equal(new Func<Int32>(() => 5).Invoke(), 5);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsTypeOfToDelegete()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => 5, Is.TypeOf<int>());
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
        public void TestAssertThat()
        {
            Assert.IsType<int>(new Func<Int32>(() => 5).Invoke());
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsTrue()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(true, Is.True);
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
        public void TestAssertThat()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsFalse()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(false, Is.False);
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
        public void TestAssertThat()
        {
            Assert.False(false);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsArgumentNullException()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new ArgumentNullException(), Throws.ArgumentNullException);
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
        public void TestAssertThat()
        {
            Assert.Throws<ArgumentNullException>(new Action(() => throw new ArgumentNullException()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsArgumentException()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new ArgumentException(), Throws.ArgumentException);
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
        public void TestAssertThat()
        {
            Assert.Throws<ArgumentException>(new Action(() => throw new ArgumentException()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsInvalidOperationException()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new InvalidOperationException(), Throws.InvalidOperationException);
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
        public void TestAssertThat()
        {
            Assert.Throws<InvalidOperationException>(new Action(() => throw new InvalidOperationException()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsNothing()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => Console.WriteLine(""hello""), Throws.Nothing);
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
        public void TestAssertThat()
        {
            new Action(() => Console.WriteLine(""hello"")).Invoke();
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsTypeOf()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new Exception(), Throws.TypeOf<Exception>());
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
        public void TestAssertThat()
        {
            Assert.Throws<Exception>(new Action(() => throw new Exception()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsInstanceOf()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new Exception(), Throws.InstanceOf<Exception>());
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
        public void TestAssertThat()
        {
            Assert.Throws<Exception>(new Action(() => throw new Exception()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatThrowsArgumentNullExceptionWithDetails()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(() => throw new ArgumentNullException(), Throws.ArgumentNullException.With.Message.Not.Null);
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
        public void TestAssertThat()
        {
            Assert.Throws<ArgumentNullException>(new Action(() => throw new ArgumentNullException()));
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsNoEqualTo()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(5, Is.Not.EqualTo(5));
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
        public void TestAssertThat()
        {
            Assert.NotEqual(5, 5);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(7, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsNotTrue()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(false, Is.Not.True);
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
        public void TestAssertThat()
        {
            Assert.False(false);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsNotFalse()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(true, Is.Not.False.And.TypeOf<bool>());
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
        public void TestAssertThat()
        {
            Assert.True(true);
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThatIsEmpty()
        {
            var source =
@"using NUnit.Framework;
using System;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public void TestAssertThat()
        {
            Assert.That(new int[] { }, Is.Empty);
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
        public void TestAssertThat()
        {
            Assert.Empty(new int[] { });
        }
    }
}";

            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThat");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }


    }
}
