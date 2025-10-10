namespace Xunit.Isolation.Tests;

public class FixtureTest
{
    public class TestFixture
    {
        public int FixtureValue;
    }

    public class InnerClass1 : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InnerClass1(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [SkippableFact]
        public void Test()
        {
            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }
    }

    public class InnerClass2 : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InnerClass2(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [SkippableFact]
        public void Test()
        {
            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }
    }

    public class InnerClass3 : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InnerClass3(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [SkippableFact]
        public void Test()
        {
            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }
    }

    public class InnerClass4 : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InnerClass4(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [SkippableFact]
        public void Test()
        {
            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }
    }
}
