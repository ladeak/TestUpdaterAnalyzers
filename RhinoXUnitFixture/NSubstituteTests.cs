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

        [Fact]
        public void OutVariableArgs()
        {
            var mock = Substitute.For<IValidator>();
            mock.TryValidate(Arg.Any<Request>(), out Arg.Any<bool>()).Returns(c => { c[1] = true; return true; });
            var sut = new BusinessLogic(mock);
            var result = sut.TryCalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void OutVariable()
        {
            var mock = Substitute.For<IValidator>();
            mock.TryValidate(new Request(), out var dummy).Returns(c => { c[1] = true; return true; });
            var sut = new BusinessLogic(mock);
            var result = sut.TryCalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void Received()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            mock.Received().Validate(Arg.Any<Request>());
        }

        [Fact]
        public void ReceivedAndNotReceived()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            mock.Received().Validate(Arg.Any<Request>());
            mock.DidNotReceive().TryValidate(Arg.Any<Request>(), out Arg.Any<bool>());
        }

        [Fact]
        public void PropertyBehavior()
        {
            var mock = Substitute.For<IValidator>();
            mock.IsEmptyNameValid = true;
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }

        [Fact]
        public void ExpectOnProperty()
        {
            var mock = Substitute.For<IValidator>();
            mock.IsEmptyNameValid.Returns(true);
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }
    }
}
