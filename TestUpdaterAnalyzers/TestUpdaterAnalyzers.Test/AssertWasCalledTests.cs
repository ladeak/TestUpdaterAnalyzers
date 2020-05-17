using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class AssertWasCalledTests : CodeFixVerifier
    {
        [TestMethod]
        public void AssertWasCalled()
        {
            var test = @"
using Rhino.Mocks;
using Xunit;
using SampleBusinessLogic;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.AssertWasCalled(x => x.Validate(Arg<Request>.Is.Anything));
        }
    }
}
";


            var expectedSource = @"
using NSubstitute;
using Rhino.Mocks;
using Xunit;
using SampleBusinessLogic;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.Received().Validate(NSubstitute.Arg.Any<Request>());
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void AssertWasNotCalled()
        {
            var test = @"
using Rhino.Mocks;
using Xunit;
using SampleBusinessLogic;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.AssertWasNotCalled(x => x.TryValidate(Arg<Request>.Is.Anything, out Arg<bool>.Out(true).Dummy));
        }
    }
}
";


            var expectedSource = @"
using NSubstitute;
using Rhino.Mocks;
using Xunit;
using SampleBusinessLogic;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenValid_IdCalculated()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.DidNotReceive().TryValidate(NSubstitute.Arg.Any<Request>(), out NSubstitute.Arg.Any<bool>());
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new TestUpdaterAnalyzersCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TestUpdaterAnalyzersAnalyzer();
        }
    }
}
