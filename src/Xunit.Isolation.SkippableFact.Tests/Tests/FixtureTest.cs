using System.Collections.Generic;
using System.Runtime.Loader;

namespace Xunit.Isolation.SkippableFact.Tests.Tests;

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

        [SkippableFact]
        public void Test()
        {
            var fixtureLoadContext = AssemblyLoadContext.GetLoadContext(_fixture.GetType().Assembly);
            var thisLoadContext = AssemblyLoadContext.GetLoadContext(GetType().Assembly);

            Assert.NotEqual(AssemblyLoadContext.Default, fixtureLoadContext);
            Assert.NotEqual(AssemblyLoadContext.Default, thisLoadContext);
            Assert.Equal(fixtureLoadContext, thisLoadContext);

            _fixture.FixtureValue++;
            Assert.Equal(1, _fixture.FixtureValue);
        }

        public static IEnumerable<object[]> ParameterClassTheoryTestMemberData =>
        [
            [new ParameterClass(1)],
            [new ParameterClass(2)],
            [new ParameterClass(3)],
            [new ParameterClass(4)],
        ];

        [SkippableTheory]
        [MemberData(nameof(ParameterClassTheoryTestMemberData))]
        public void ParameterClassTheoryTest(ParameterClass param)
        {
            var thisLoadContext = AssemblyLoadContext.GetLoadContext(GetType().Assembly);
            var paramLoadContext = AssemblyLoadContext.GetLoadContext(param.GetType().Assembly);

            Assert.NotEqual(AssemblyLoadContext.Default, thisLoadContext);
            Assert.NotEqual(AssemblyLoadContext.Default, paramLoadContext);
            Assert.Equal(thisLoadContext, paramLoadContext);

            param.StaticValue++;
            Assert.Equal(param.Value, param.StaticValue);
        }

        public static IEnumerable<object[]> ParameterStructTheoryTestMemberData =>
        [
            [new ParameterStruct(1)],
            [new ParameterStruct(2)],
            [new ParameterStruct(3)],
            [new ParameterStruct(4)],
        ];

        [SkippableTheory]
        [MemberData(nameof(ParameterStructTheoryTestMemberData))]
        public void ParameterStructTheoryTest(ParameterStruct param)
        {
            var thisLoadContext = AssemblyLoadContext.GetLoadContext(GetType().Assembly);
            var paramLoadContext = AssemblyLoadContext.GetLoadContext(param.GetType().Assembly);

            Assert.NotEqual(AssemblyLoadContext.Default, thisLoadContext);
            Assert.NotEqual(AssemblyLoadContext.Default, paramLoadContext);
            Assert.Equal(thisLoadContext, paramLoadContext);

            param.StaticValue++;
            Assert.Equal(param.Value, param.StaticValue);
        }
    }

    public class InnerClass1(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass2(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass3(TestFixture fixture) : InnerClass(fixture);
    public class InnerClass4(TestFixture fixture) : InnerClass(fixture);
}
