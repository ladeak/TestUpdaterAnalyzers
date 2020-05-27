using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class IgnoreArgumentsFixTests : CodeFixVerifier
    {
        [TestMethod]
        public void IgnoreArgumentsBeforeReturn()
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
        public void IgnoreArguments()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Expect(x => x.Validate(new Request())).IgnoreArguments().Return(true);
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
        public void IgnoreArguments()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(new Request()).ReturnsForAnyArgs(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void IgnoreArgumentsAfterReturn()
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
        public void IgnoreArguments()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Expect(x => x.Validate(new Request())).Return(true).IgnoreArguments();
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
        public void IgnoreArguments()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(new Request()).ReturnsForAnyArgs(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.Equal(5, result);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void IgnoreArgumentsOnThrow()
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
        public void IgnoreArguments()
        {
            var mock = MockRepository.GenerateStub<IValidator>();
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).IgnoreArguments().Throw(new ArgumentException(""request""));
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
        public void IgnoreArguments()
        {
            var mock = Substitute.For<IValidator>();
            mock.Validate(NSubstitute.Arg.Any<Request>()).ThrowsForAnyArgs(new ArgumentException(""request""));
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
