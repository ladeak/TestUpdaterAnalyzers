using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [Fact]
        public void TestAssertTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void TestAssertAreEqual()
        {
            Assert.Equal(0, 0);
        }

        [Fact]
        public void TestAssertNull()
        {
            Assert.NotNull(new object());
        }

        [Fact]
        public void TestAssertSame()
        {
            var o = new object();
            Assert.Same(o, o);
        }

        [Fact]
        public void TestAssertEmpty()
        {
            Assert.Empty(new List<object>());
            Assert.Equal("", string.Empty);
        }

        [Fact]
        public void TestAssertZero()
        {
            int actual = 0;
            Assert.Equal(0, actual);
            actual = 1;
            Assert.NotEqual(0, actual);
        }

        [Fact]
        public void TestAssertPassFail()
        {
            Assert.True(true);
        }

        [Fact]
        public async Task TestAssertThrows()
        {
            Assert.Throws<Exception>(new Action(() => throw new Exception()));
            Assert.Throws<Exception>(new Action(() => { throw new Exception(); }));
            Console.WriteLine("hello");
            await Assert.ThrowsAsync<Exception>(async () => throw new Exception());
            await Assert.ThrowsAsync<Exception>(async () => { throw new Exception(); });
            Console.WriteLine("hello");
        }

        [Fact]
        public void TestAssertContains()
        {
            Assert.Contains(5, new[] { 1, 3, 5, 7 });
        }

        [Fact]
        public void TestAssertInstanceOf()
        {
            Assert.IsAssignableFrom<Exception>(new ArgumentNullException());
        }


        [Fact]
        public void TestAssertIsNotInstanceOf()
        {
            Assert.False(new Exception() is ArgumentNullException);
        }

        [Fact]
        public void TestAssertIsAssignableFrom()
        {
            Assert.True(new Exception().GetType().IsAssignableFrom(typeof(ArgumentNullException)));
            Assert.False(new DivideByZeroException().GetType().IsAssignableFrom(typeof(ArgumentNullException)));
        }

        [Fact]
        public void TestAssertThat()
        {
            Assert.True(new Func<bool>(() => true).Invoke());
            Assert.Throws<Exception>(new Action(() => throw new Exception()));
            Assert.IsType<int>(new Func<Int32>(() => 5).Invoke());

            Assert.Throws<ArgumentNullException>(new Action(() => throw new ArgumentNullException()));
        }

    }
}