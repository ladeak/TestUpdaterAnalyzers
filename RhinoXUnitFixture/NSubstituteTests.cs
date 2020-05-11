using NSubstitute;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class NSubstituteTests
    {
        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });
            Assert.Equal(5, result);
        }
    }
}
