using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace TestUpdaterAnalyzers.Test
{
    [TestClass]
    public class WhenCalledFixTests : CodeFixVerifier
    {
        [TestMethod]
        public void WhenCalledUseReturns()
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
        public void WhenCalled()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            bool flag = false;
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).WhenCalled(x=>
            {
                if (x.Arguments[0] != null)
                    flag = true;
            }).Return(true);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.True(flag);
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
        public void WhenCalled()
        {
            var mock = Substitute.For<IValidator>();
            bool flag = false;
            mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(a0 =>
            {
                if (a0[0] != null)
                    flag = true;
                return true;
            });
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.True(flag);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void WhenCalledWithReturnValueUseReturns()
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
        public void WhenCalled()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            bool flag = false;
            mock.Expect(x => x.Validate(Arg<Request>.Is.Anything)).WhenCalled(x=>
            {
                if (x.Arguments[0] != null)
                    flag = true;
                x.ReturnValue = true;
            }).Return(default);
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.True(flag);
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
        public void WhenCalled()
        {
            var mock = Substitute.For<IValidator>();
            bool flag = false;
            mock.Validate(NSubstitute.Arg.Any<Request>()).Returns(a0 =>
            {
                if (a0[0] != null)
                    flag = true;
                return true;
            });
            var sut = new BusinessLogic(mock);
            var result = sut.CalculateId(new Request() { Age = 1, Height = 1, Name = ""test"" });
            Assert.True(flag);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void WhenCalledVoid()
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
        public void WhenCalled()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            bool flag = false;
            mock.Stub(x => x.Run()).WhenCalled(x => flag = true);
            new BusinessLogic(mock).Run();
            Assert.True(flag);
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
        public void WhenCalled()
        {
            var mock = Substitute.For<IValidator>();
            bool flag = false;
            mock.When(x => x.Run()).Do(x => flag = true);
            new BusinessLogic(mock).Run();
            Assert.True(flag);
        }
    }
}
";
            VerifyCSharpFix(test, expectedSource, allowNewCompilerDiagnostics: false);
        }

        [TestMethod]
        public void WhenCalledWithIgnoreArguments()
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
        public void WhenCalled()
        {
            var mock = MockRepository.GenerateMock<IValidator>();
            bool flag = false;
            mock.Stub(x => x.Run()).WhenCalled(x => flag = true).IgnoreArguments();
            new BusinessLogic(mock).Run();
            Assert.True(flag);
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
        public void WhenCalled()
        {
            var mock = Substitute.For<IValidator>();
            bool flag = false;
            mock.WhenForAnyArgs(x => x.Run()).Do(x => flag = true);
            new BusinessLogic(mock).Run();
            Assert.True(flag);
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
