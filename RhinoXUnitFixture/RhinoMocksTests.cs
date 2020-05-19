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

        [Fact]
        public void Repeat()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Repeat.Twice().Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void OutVariableWithOutRef()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.TryValidate(new Request(), out var dummy)).OutRef(true).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.TryCalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void OutVariable()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.TryValidate(Arg<Request>.Is.Anything, out Arg<bool>.Out(true).Dummy)).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.TryCalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void VerifyAllExpectations()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            mock.VerifyAllExpectations();
        }

        [Fact]
        public void AssertWasCalled()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            mock.AssertWasCalled(x => x.Validate(Arg<Request>.Is.Anything));
            mock.AssertWasNotCalled(x => x.TryValidate(Arg<Request>.Is.Anything, out Arg<bool>.Out(true).Dummy));
        }

        [Fact]
        public void PropertyBehavior()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.IsEmptyNameValid).PropertyBehavior();
            mock.IsEmptyNameValid = true;
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }

        [Fact]
        public void ExpectOnProperty()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.IsEmptyNameValid).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }

        [Fact]
        public void Arg_ArgumentNull()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Null)).Return(true);
            var sut = new BusinessLogic(mock);
            Assert.Throws<NullReferenceException>(() => sut.CalculateId(null));
        }

        [Fact]
        public void Arg_ArgumentNotNull()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.NotNull)).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

        [Fact]
        public void Arg_ArgumentEqual()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            var request = new Request() { Age = 1, Height = 1, Name = "test" };
            mock.Expect(x => x.Validate(Arg<Request>.Is.Equal(request))).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(request);
            Assert.Equal(5, result);
        }

        [Fact]
        public void Arg_ArgumentSame()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            var request = new Request() { Age = 1, Height = 1, Name = "test" };
            mock.Expect(x => x.Validate(Arg<Request>.Is.Same(request))).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(request);
            Assert.Equal(5, result);
        }

        [Fact]
        public void Arg_ArgumentMatches()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Matches(y => y.Name == "test"))).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }

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