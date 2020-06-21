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
        public async Task Throws()
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
            Assert.Throws<Exception>(() => throw new Exception());
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
            Assert.Throws<Exception>(new Action(() => throw new Exception()));
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThrowsBlock()
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
            Assert.Throws<Exception>(() => { throw new Exception(); }, ""some message"");
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
            Assert.Throws<Exception>(new Action(() => { throw new Exception(); }));
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task DoesNotThrowBlock()
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
            Assert.DoesNotThrow(() => Console.WriteLine(""hello""), ""some message"");
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
        public async Task DoesNotThrowBlockContext()
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
            }, ""some message"");
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

        [TestMethod]
        public async Task DoesNotThrowWithContextAround()
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
            int i = 0;
            Assert.DoesNotThrow(() =>
            {
                Console.WriteLine(""hello"");
                Console.WriteLine(""world"");
            });
            i++;
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
            int i = 0;
            Console.WriteLine(""hello"");
            Console.WriteLine(""world"");
            i++;
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(8, 9).WithArguments("TestAssertNotThrows");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThrowsAsync()
        {
            var source =
@"using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public async Task TestAssertThrowsAsync()
        {
            await Task.Yield();
            Assert.ThrowsAsync<Exception>(async () => throw new Exception(), ""some message"");
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestAssertThrowsAsync()
        {
            await Task.Yield();
            await Assert.ThrowsAsync<Exception>(async () => throw new Exception());
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(9, 9).WithArguments("TestAssertThrowsAsync");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task ThrowsAsyncBlock()
        {
            var source =
@"using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public async Task TestAssertThrowsAsync()
        {
            await Task.Yield();
            Assert.ThrowsAsync<Exception>(async () =>
            {
                throw new Exception();
            }, ""some message"");
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestAssertThrowsAsync()
        {
            await Task.Yield();
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                throw new Exception();
            });
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(9, 9).WithArguments("TestAssertThrowsAsync");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task DoesNotThrowAsync()
        {
            var source =
@"using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public async Task TestAssertNotThrowsAsync()
        {
            Assert.DoesNotThrowAsync(async () => Console.WriteLine(""hello""));
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestAssertNotThrowsAsync()
        {
            Console.WriteLine(""hello"");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(9, 9).WithArguments("TestAssertNotThrowsAsync");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task DoesNotThrowAsyncBlock()
        {
            var source =
@"using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Test]
        public async Task TestAssertNotThrowsAsync()
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                await Task.Yield();
                Console.WriteLine(""hello"");
                Console.WriteLine(""world"");
            });
        }
    }
}";

            var fixtest =
@"using Xunit;
using System;
using System.Threading.Tasks;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestAssertNotThrowsAsync()
        {
            await Task.Yield();
            Console.WriteLine(""hello"");
            Console.WriteLine(""world"");
        }
    }
}";

            var expected = Verify.Diagnostic("ADNXunitConverterAnalyzer").WithLocation(9, 9).WithArguments("TestAssertNotThrowsAsync");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }
}
