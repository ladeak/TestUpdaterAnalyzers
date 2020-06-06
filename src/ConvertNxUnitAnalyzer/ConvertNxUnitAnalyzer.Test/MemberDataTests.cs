using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    ConvertNxUnitAnalyzer.ConvertNxUnitAnalyzer,
    ConvertNxUnitAnalyzer.ConvertNxUnitCodeFixProvider>;

namespace ConvertNxUnitAnalyzer.Test
{
    [TestClass]
    public class MemberDataTests
    {
        [TestMethod]
        public async Task TestCaseSourceAttribute_DiagnosticWarning()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class NUnitTests
    {
        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData(""v"", 3);
                yield return new TestCaseData(""a"", 2);
                yield return new TestCaseData(""l"", 4);
            }
        }

        [TestCaseSource(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(18, 9).WithArguments("TestCaseSource");
            await VerifyCodeFix.VerifyAnalyzerAsync(source, expected);
        }


        [TestMethod]
        public async Task NameOfAttributeReplacedWithMemberData()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData(""v"", 3);
                yield return new TestCaseData(""a"", 2);
                yield return new TestCaseData(""l"", 4);
            }
        }

        [TestCaseSource(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var fixtest =
@"using Xunit;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[] { ""v"", 3 };
                yield return new object[] { ""a"", 2 };
                yield return new object[] { ""l"", 4 };
            }
        }
      
        [Theory]
        [MemberData(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(18, 9).WithArguments("TestCaseSource");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task StringNameAttributeReplacedWithMemberData()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData(""v"", 3);
                yield return new TestCaseData(""a"", 2);
                yield return new TestCaseData(""l"", 4);
            }
        }

        [TestCaseSource(""TestData"")]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var fixtest =
@"using Xunit;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[] { ""v"", 3 };
                yield return new object[] { ""a"", 2 };
                yield return new object[] { ""l"", 4 };
            }
        }
      
        [Theory]
        [MemberData(""TestData"")]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(18, 9).WithArguments("TestCaseSource");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }

        [TestMethod]
        public async Task TestCaseSourceAndTestAttributeReplacedWithMemberData()
        {
            var source =
@"using NUnit.Framework;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData(""v"", 3);
                yield return new TestCaseData(""a"", 2);
                yield return new TestCaseData(""l"", 4);
            }
        }

        [Test]
        [TestCaseSource(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var fixtest =
@"using Xunit;
using System.Collections.Generic;

namespace NUnitToXUnitTests
{
    public class UnitTests
    {
        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[] { ""v"", 3 };
                yield return new object[] { ""a"", 2 };
                yield return new object[] { ""l"", 4 };
            }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestCaseSource(string value, int value2)
        {
        }
    }
}";

            var expected = Verify.Diagnostic("ADConvertNxUnitAnalyzer").WithLocation(18, 9).WithArguments("TestCaseSource");
            await VerifyCodeFix.VerifyFixAsync(source, fixtest, expected);
        }
    }
}
