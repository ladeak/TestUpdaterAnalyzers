using NUnit.Framework;

namespace NUnitToXUnitTests
{
    public class NUnitTests
    {
        [Test]
        public void Test()
        {
        }

        [TestCase("value")]
        [TestCase("value1")]
        public void TestCase(string value)
        {
        }
    }
}