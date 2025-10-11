using System.Runtime.Loader;

namespace Xunit.Isolation.SkippableFact.Tests.Tests;

public class AssemblyLoadContextReuseTest
{
    private static int StaticValue;

    [Collection("AssemblyLoadContextReuseTest")]
    public abstract class InnerClass
    {
        protected abstract AssemblyLoadContextIndependentStorage<bool> _isInitialized { get; }
        protected abstract AssemblyLoadContextIndependentStorage<int> _initCount { get; }

        [Fact]
        public void FactTest()
        {
            var thisLoadContext = AssemblyLoadContext.GetLoadContext(GetType().Assembly);

            Assert.NotEqual(AssemblyLoadContext.Default, thisLoadContext);
            StaticValue++;

            if (_isInitialized.Value)
                Assert.True(1 < StaticValue && StaticValue <= 4);
            else
                Assert.Equal(1, StaticValue);

            _isInitialized.Value = true;
            _initCount.Value++;
        }
    }

    [IsolationContextConfig(isolationId: "AssemblyLoadContextReuseTest.A")]
    public abstract class InnerClassA : InnerClass
    {
        protected override AssemblyLoadContextIndependentStorage<bool> _isInitialized => new("AssemblyLoadContextReuseTest.A.IsInitialized");
        protected override AssemblyLoadContextIndependentStorage<int> _initCount => new("AssemblyLoadContextReuseTest.A.InitCount");
    }

    [IsolationContextConfig(isolationId: "AssemblyLoadContextReuseTest.B")]
    public abstract class InnerClassB : InnerClass
    {
        protected override AssemblyLoadContextIndependentStorage<bool> _isInitialized => new("AssemblyLoadContextReuseTest.B.IsInitialized");
        protected override AssemblyLoadContextIndependentStorage<int> _initCount => new("AssemblyLoadContextReuseTest.B.InitCount");
    }

    public class InnerClassA1 : InnerClassA;
    public class InnerClassA2 : InnerClassA;
    public class InnerClassA3 : InnerClassA;
    public class InnerClassA4 : InnerClassA;
    public class InnerClassB1 : InnerClassB;
    public class InnerClassB2 : InnerClassB;
    public class InnerClassB3 : InnerClassB;
    public class InnerClassB4 : InnerClassB;
}
