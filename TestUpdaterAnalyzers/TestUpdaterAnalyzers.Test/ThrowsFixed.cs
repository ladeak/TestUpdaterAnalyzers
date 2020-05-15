using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class ThrowsFixed : CodeFixVerifier
    {
        [TestMethod]
        public void GivenRhinoMockShowsItemsToFix()
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
        public void WhenNull_ThrowsException()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Throw(new ArgumentException(""request""));
            var sut = new BusinessLogic(mock);
            Assert.Throws<ArgumentException>(() => sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" }));
        }
    }
}
";
            var updateMethodDiagnostics = new DiagnosticResult
            {
                Id = TestUpdaterAnalyzersAnalyzer.RhinoUsageId,
                Message = "Update 'WhenNull_ThrowsException' method to NSubsitute",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 9) }
            };

            VerifyCSharpDiagnostic(test, updateMethodDiagnostics);
        }

        [TestMethod]
        public void GivenRhinoMockReplacesToNSubstitute()
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
        public void WhenNull_ThrowsException()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Throw(new ArgumentException(""request""));
            var sut = new BusinessLogic(mock);
            Assert.Throws<ArgumentException>(() => sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" }));
        }
    }
}
";


            var expectedSource = @"
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Rhino.Mocks;
using Xunit;
using SampleBusinessLogic;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        [Fact]
        public void WhenNull_ThrowsException()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(NSubstitute.Arg.Any<Request>()).Throws(new ArgumentException(""request""));
            var sut = new BusinessLogic(mock);
            Assert.Throws<ArgumentException>(() => sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" }));
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
