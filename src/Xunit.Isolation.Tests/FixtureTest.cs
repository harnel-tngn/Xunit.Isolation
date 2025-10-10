namespace Xunit.Isolation.Tests;

public class FixtureTest
{
    public class TestFixture
    {
        public int FixtureValue;
    }

    public abstract class InnerClass : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public InnerClass(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Test()
        {
            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }
    }

    public class InnerClass1(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass2(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass3(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass4(TestFixture fixture) : InnerClass(fixture);
}
