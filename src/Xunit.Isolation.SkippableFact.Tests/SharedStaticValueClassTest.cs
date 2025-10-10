namespace Xunit.Isolation.Tests;

public class SharedStaticValueClassTest
{
    public static int StaticValue;

    public class InnerClass1
    {
        [SkippableFact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass2
    {
        [SkippableFact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass3
    {
        [SkippableFact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass4
    {
        [SkippableFact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }
}
