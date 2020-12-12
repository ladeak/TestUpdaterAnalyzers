using Xunit;

namespace NUnitToXUnitTests
{
    public class xUnitTestsFixtureSetupData
    {
        public xUnitTestsFixtureSetupData()
        {
            _param = true;
        }

        public bool _param;
    }

    public class xUnitTestsFixtureSetup : IClassFixture<xUnitTestsFixtureSetupData>
    {
        private xUnitTestsFixtureSetupData _fixture;

        public xUnitTestsFixtureSetup(xUnitTestsFixtureSetupData fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Test()
        {
            Assert.True(_fixture._param);
        }
    }
}