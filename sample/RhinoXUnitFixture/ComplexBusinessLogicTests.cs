using Rhino.Mocks;
using SampleBusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RhinoXUnitFixture
{
    public class ComplexBusinessLogicTests
    {
        [Fact]
        public void GivenNotNull_Construct_DoesNotThrow()
        {
            var ex = Record.Exception(() => new ComplexBusinessLogic(
                MockRepository.GenerateMock<IValidator>(),
                MockRepository.GenerateMock<INameProvider>(),
                MockRepository.GenerateMock<ILogger<ComplexBusinessLogic>>()));
            Assert.Null(ex);
        }

        [Fact]
        public void CalculateId_Returns_NumberEs_Multiplied_Age_Height()
        {
            // Arrange
            var validatorStub = MockRepository.GenerateStub<IValidator>();
            validatorStub.Stub(x => x.TryValidate(Arg<Request>.Matches(y => y.Name == "test"), out Arg<bool>.Out(true).Dummy)).Return(true);

            var nameProviderMock = MockRepository.GenerateMock<INameProvider>();
            nameProviderMock.Expect(x => x.GetFullName("test")).IgnoreArguments().Return("eee").Repeat.Any();
            nameProviderMock.Expect(x => x.Initialized).Return(true);

            var loggerMock = MockRepository.GenerateMock<ILogger<ComplexBusinessLogic>>();
            var sut = new ComplexBusinessLogic(validatorStub, nameProviderMock, loggerMock);
            
            // Act
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = "test" });

            // Assert
            Assert.Equal(3, result);
            nameProviderMock.VerifyAllExpectations();
            loggerMock.AssertWasCalled(x => x.Log(Arg<string>.Is.NotNull));
        }

    }
}
