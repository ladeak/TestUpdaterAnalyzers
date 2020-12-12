using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class BaseClassAndCtorTests
    {
        [TestMethod]
        public async Task TestClass_WithBase()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitBase { }

    public class NUnitTests : NUnitBase
    {
        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}";

            await VerifyCodeFix.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task TestClass_WithCtor()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public UnitTests()
        { }

        [Test]
        public void Test1()
        {
        }
    }
}";

            await VerifyCodeFix.VerifyAnalyzerAsync(source);
        }
    }
}
