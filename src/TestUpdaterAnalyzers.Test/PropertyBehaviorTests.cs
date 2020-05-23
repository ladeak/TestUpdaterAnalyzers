using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class PropertyBehaviorTests : CodeFixVerifier
    {
        [TestMethod]
        public void PropertyBehavior()
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
            mock.Expect(x => x.IsEmptyNameValid).PropertyBehavior();
            mock.IsEmptyNameValid = true;
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
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
            mock.IsEmptyNameValid = true;
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void ExpectOnProperty()
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
            mock.Expect(x => x.IsEmptyNameValid).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
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
            mock.IsEmptyNameValid.Returns(true);
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            Assert.True(result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void VerifyAllOnProperty()
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
            mock.Expect(x => x.IsEmptyNameValid = true);
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
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
            var sut = new BusinessLogic(mock);
            var result = sut.IsEmptyNameAllowed();
            mock.Received().IsEmptyNameValid = true;
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
