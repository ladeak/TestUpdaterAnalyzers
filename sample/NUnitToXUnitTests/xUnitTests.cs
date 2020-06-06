using System.Collections.Generic;
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

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[] { "v", 3 };
                yield return new object[] { "a", 2 };
                yield return new object[] { "l", 4 };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void MemberData(string value, int value2)
        {
        }
    }
}