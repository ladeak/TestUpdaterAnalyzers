using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class VerifyAllExpectationsTests : CodeFixVerifier
    {
        [TestMethod]
        public void VerifyAllExpectations()
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
            mock.VerifyAllExpectations();
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
            mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(true);
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
        public void VerifyAllExpectationsMultipleExpects()
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
            mock.Expect(x => x.TryValidate(Arg<Request>.Is.Anything, out Arg<bool>.Out(true).Dummy)).Return(true);
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.VerifyAllExpectations();
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
            mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(true);
            mock.TryValidate(NSubstitute.Arg.Any<Request>(), out NSubstitute.Arg.Any<bool>()).Returns(a0 => { a0[1] = true; return true; });
            var sut = new BusinessLogic(mock);
            sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            mock.Received().Validate(NSubstitute.Arg.Any<Request>());
            mock.Received().TryValidate(NSubstitute.Arg.Any<Request>(), out NSubstitute.Arg.Any<bool>());
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
