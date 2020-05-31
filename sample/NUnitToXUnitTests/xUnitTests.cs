using Xunit;

namespace NUnitToXUnitTests
{
    public class xUnitTests
    {
        [Fact]
        public void Test()
        {
        }

        [Theory]
        [InlineData("value")]
        [InlineData("value1")]
        public void TestCase(string value)
        {
        }
    }
}