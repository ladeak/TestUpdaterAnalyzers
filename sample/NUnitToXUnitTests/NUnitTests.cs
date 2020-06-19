using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [Test]
        public void TestAssertTrue()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void TestAssertAreEqual()
        {
            Assert.AreEqual(0, 0);
        }

        [Test]
        public void TestAssertNull()
        {
            Assert.IsNull(null);
        }

        [Test]
        public void TestAssertSame()
        {
            var o = new object();
            Assert.AreSame(o, o);
        }

        [Test]
        public void TestAssertEmpty()
        {
            Assert.IsEmpty(new List<object>());
            Assert.IsEmpty("");
        }

        [Test]
        public void TestAssertZero()
        {
            Assert.Zero(0);
            Assert.NotZero(1);
        }

        [Test]
        public void TestAssertPassFail()
        {
            Assert.Pass();
        }

        [Test]
        public async Task TestAssertThrows()
        {
            Assert.Throws<Exception>(() => new Exception());
            Assert.DoesNotThrow(() => Console.WriteLine("hello"));

            Assert.DoesNotThrow(() =>
            {
                Console.WriteLine("hello");
                Console.WriteLine("world");
            });
            Assert.ThrowsAsync<Exception>(async () => new Exception());
            Assert.DoesNotThrowAsync(async () => Console.WriteLine("hello"));
        }

        //Assert.That
        //Assert.Contains
        //Assert.Throws
        //Assert.ThrowsAsync
        //Assert.DoesNotThrow
        //Assert.DoesNotThrowAsync
        //Assert.IsAssignableFrom
        //Assert.IsInstanceOf
    }
}