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
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyFileNoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

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
        public void WhenValid_IdCalculated()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            var updateMethodDiagnostics = new DiagnosticResult
            {
                Id = TestUpdaterAnalyzersAnalyzer.RhinoUsageId,
                Message = "Update 'WhenValid_IdCalculated' method to NSubsitute",
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
            var mock = MockRepository.GenerateMock<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
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
            mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: true);
        }


        [TestMethod]
        public void GivenRhinoMockPrivateFieldToFix()
        {
            var test = @"
using Rhino.Mocks;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private readonly IValidator _mock = MockRepository.GenerateMock<IValidator>();

        [Fact]
        public void WhenValid_IdCalculated()
        {
            _mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(_mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            var updateMethodDiagnostics = new DiagnosticResult
            {
                Id = TestUpdaterAnalyzersAnalyzer.RhinoUsageId,
                Message = "Update 'WhenValid_IdCalculated' method to NSubsitute",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 9) }
            };

            VerifyCSharpDiagnostic(test, updateMethodDiagnostics);
        }


        [TestMethod]
        public void GivenRhinoMockPrivateFieldReplacesNSubsitute()
        {
            var test = @"
using Rhino.Mocks;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private readonly IValidator _mock = MockRepository.GenerateMock<IValidator>();

        [Fact]
        public void WhenValid_IdCalculated()
        {
            _mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(_mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            var expectedSource = @"
using NSubstitute;
using Rhino.Mocks;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private readonly IValidator _mock = Substitute.For<IValidator>();

        [Fact]
        public void WhenValid_IdCalculated()
        {
            _mock.Validate(Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(_mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: true);
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
