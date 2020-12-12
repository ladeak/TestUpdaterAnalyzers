using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTestsFixtureSetup
    {
        private bool _param;
        [OneTimeSetUp]
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
}