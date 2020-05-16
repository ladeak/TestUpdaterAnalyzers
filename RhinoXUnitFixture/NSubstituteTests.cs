using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SampleBusinessLogic;
using System;
using Xunit;

namespace RhinoXUnitFixture
{
    public class NSubstituteTests
    {
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Throws(new ArgumentException("request"));
            var sut = new BusinessLogic(mock);
            Assert.Throws<ArgumentException>(() => sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" }));
        }

        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void IgnoreArguments()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(new Request()).ReturnsForAnyArgs(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }
    }
}
