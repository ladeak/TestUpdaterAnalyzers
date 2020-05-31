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
    public class ClassScopeExpectReturn : CodeFixVerifier
    {
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
            _mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(_mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void GivenRhinoMockPropertyReplacesNSubsitute()
        {
            var test = @"
using Rhino.Mocks;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private IValidator Mock { get; set; } = MockRepository.GenerateMock<IValidator>();

        [Fact]
        public void WhenValid_IdCalculated()
        {
            Mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).Return(true);
            var sut = new BusinessLogic(Mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            var expectedSource = @"
using NSubstitute;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private IValidator Mock { get; set; } = Substitute.For<IValidator>();

        [Fact]
        public void WhenValid_IdCalculated()
        {
            Mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(Mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void GivenRhinoMockPrivateFieldWithSetupReplacesNSubsitute()
        {
            var test = @"
using Rhino.Mocks;
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private IValidator _mock;

        public RhinoMocksTests()
        {
            _mock = MockRepository.GenerateMock<IValidator>();
        }

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
using SampleBusinessLogic;
using Xunit;

namespace RhinoXUnitFixture
{
    public class RhinoMocksTests
    {
        private IValidator _mock;

        public RhinoMocksTests()
        {
            _mock = Substitute.For<IValidator>();
        }

        [Fact]
        public void WhenValid_IdCalculated()
        {
            _mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(true);
            var sut = new BusinessLogic(_mock);
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
