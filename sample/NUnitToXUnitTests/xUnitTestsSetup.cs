using System;
using Xunit;

namespace NUnitToXUnitTests
{
    public class xUnitTestsSetup : IDisposable
    {
        private bool _param;

        public xUnitTestsSetup()
        {
            _param = true;
        }

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
}