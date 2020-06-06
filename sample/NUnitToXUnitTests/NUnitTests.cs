using NUnit.Framework;
using System.Collections.Generic;

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

        [Test]
        [TestCase("value")]
        [TestCase("value1")]
        public void TestAndTestCase(string value)
        {
        }

        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData("v", 3);
                yield return new TestCaseData("a", 2);
                yield return new TestCaseData("l", 4);
            }
        }

        [TestCaseSource(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}