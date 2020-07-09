using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTestsSetup
    {
        private bool _param;
        [SetUp]
        public void Setup()
        {
            _param = true;
        }

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
}