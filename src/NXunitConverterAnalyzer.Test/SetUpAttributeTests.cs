using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class SetUpAttributeTests
    {
        [TestMethod]
        public async Task SetUp_Converted_Ctor()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class MyTestClass
    {
        private bool _param;
        [SetUp]
        public void Setup()
        {
            _param = true;
        }

        [Test]
        public void Test()
        {
            Assert.True(_param);
        }
    }
}";


            var fixtest =
@"using Xunit;

namespace NUnitToXUnitTests
{
    public class MyTestClass
    {
        private bool _param;

        public MyTestClass()
        {
            _param = true;
        }

        [Fact]
        public void Test()
        {
            Assert.True(_param);
        }
    }
}";
            var expected = Verify.Diagnostic("NXunitConverterAnalyzer").WithLocation(14, 9).WithArguments("Test");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }

}
