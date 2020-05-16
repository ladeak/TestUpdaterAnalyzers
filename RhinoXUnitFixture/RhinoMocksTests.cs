using NSubstitute;
using Rhino.Mocks;
using SampleBusinessLogic;
using System;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Throw(new ArgumentException("request"));
            var sut = new BusinessLogic(mock);
            Assert.Throws<ArgumentException>(() => sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" }));
        }

        [Fact]
        public void WhenValid_WithStub()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Stub(x => x.Validate(new Request())).IgnoreArguments().Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void IgnoreArguments()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Stub(x => x.Validate(new Request())).IgnoreArguments().Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }


        // Repeat
        // IgnoreArguments
        // Raise
        // Out argument
        // AssertWasCalled
        // AssertWasNotCalled
        // Do
        // PropertyBehavior
        // WhenCalled
        // Stub

        // Arguments: Matches, Is (Null, NotNull, Typeof, Equal, Same), Out, Ref

        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }
    }
}