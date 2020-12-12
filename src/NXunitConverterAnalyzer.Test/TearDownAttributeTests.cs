using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    NXunitConverterAnalyzer.NXunitConverterAnalyzer,
    NXunitConverterAnalyzer.NXunitConverterFixProvider>;

namespace NXunitConverterAnalyzer.Test
{
    [TestClass]
    public class TearDownAttributeTests
    {
        [TestMethod]
        public async Task TearDown_Converted_Dispose()
        {
            var source =
@"using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class MyTestClass
    {
        private bool _param;
        [TearDown]
        public void TearDown()
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
@"using System;
using Xunit;

namespace NUnitToXUnitTests
{
    public class MyTestClass
: IDisposable
    {
        private bool _param;

        public void Dispose()
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
