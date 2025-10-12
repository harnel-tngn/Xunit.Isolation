using System.Collections.Generic;
using System.Runtime.Loader;

namespace Xunit.Isolation.Tests.Tests;

public class SharedStaticValueClassTest
{
    public static int StaticValue;

    public abstract class InnerClass
    {
#if USE_SKIPPABLE
        [SkippableFact]
#else
        [Fact]
#endif
        public void FactTest()
        {
            var thisLoadContext = AssemblyLoadContext.GetLoadContext(GetType().Assembly);

            Assert.NotEqual(AssemblyLoadContext.Default, thisLoadContext);

            StaticValue++;
            Assert.Equal(1, StaticValue);
        }

        public static IEnumerable<object[]> ParameterClassTheoryTestMemberData =>
        [
            [new ParameterClass(1)],
            [new ParameterClass(2)],
            [new ParameterClass(3)],
            [new ParameterClass(4)],
        ];

#if USE_SKIPPABLE
        [SkippableTheory]
#else
        [Theory]
#endif
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

#if USE_SKIPPABLE
        [SkippableTheory]
#else
        [Theory]
#endif
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

    public class InnerClass1 : InnerClass;
    public class InnerClass2 : InnerClass;
    public class InnerClass3 : InnerClass;
    public class InnerClass4 : InnerClass;
}
