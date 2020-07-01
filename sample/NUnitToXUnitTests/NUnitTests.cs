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
            await Task.Yield();
            Assert.Throws<Exception>(() => throw new Exception());
            Assert.DoesNotThrow(() => Console.WriteLine("hello"));
            Assert.DoesNotThrow(() =>
            {
                Console.WriteLine("hello");
                Console.WriteLine("world");
            });
            Assert.ThrowsAsync<Exception>(() => { throw new Exception(); });
            Assert.ThrowsAsync<Exception>(() => throw new Exception());
            Assert.DoesNotThrowAsync(async () => Console.WriteLine("hello"));
        }

        [Test]
        public void TestAssertContains()
        {
            Assert.Contains(5, new[] { 1, 3, 5, 7 });
        }

        [Test]
        public void TestAssertInstanceOf()
        {
            Assert.IsInstanceOf<Exception>(new ArgumentNullException());
        }

        [Test]
        public void TestAssertIsNotInstanceOf()
        {
            Assert.IsNotInstanceOf<ArgumentNullException>(new Exception());
        }

        [Test]
        public void TestAssertIsAssignableFrom()
        {
            Assert.IsAssignableFrom<ArgumentNullException>(new Exception());
            Assert.IsNotAssignableFrom<ArgumentNullException>(new DivideByZeroException());
        }

        [Test]
        public void TestAssertThat()
        {
            Assert.That(5, Is.EqualTo(5));
            Assert.That(() => 5, Is.EqualTo(5));
            Assert.That(() => 5, Is.TypeOf<int>());
            Assert.That(true, Is.True);
            Assert.That(false, Is.False);
            Assert.That(true);
            Assert.That(() => true);
            Assert.That(true, () => "error");
            Assert.That(() => true, () => "error");
            Assert.That(true, "error", "param");
            Assert.That(() => true, "error", "param");
            Assert.That(() => throw new Exception(), Throws.InstanceOf<Exception>());
            Assert.That(() => throw new ArgumentNullException(), Throws.ArgumentNullException.With.Message.Not.Null);

            Assert.That(false, Is.Not.True);
            Assert.That(new int[] { }, Is.Empty);
        }

    }
}