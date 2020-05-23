using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using TestUpdaterAnalyzers;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class GenerateStubFixed : CodeFixVerifier
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
        public void EmptyTest()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
        }
    }
}
";
            var updateMethodDiagnostics = new DiagnosticResult
            {
                Id = TestUpdaterAnalyzersAnalyzer.RhinoUsageId,
                Message = "Update 'EmptyTest' method to NSubsitute",
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
        public void WhenValid_IdCalculated()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Stub(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
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
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
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
