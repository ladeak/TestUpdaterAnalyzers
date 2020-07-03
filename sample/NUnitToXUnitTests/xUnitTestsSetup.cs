using Xunit;

namespace NUnitToXUnitTests
{
    public class xUnitTestsSetup
    {
        private bool _param;

        public xUnitTestsSetup()
        {
            _param = true;
        }

        [Fact]
        public void Test()
        {
            Assert.True(_param);
        }

    }
}