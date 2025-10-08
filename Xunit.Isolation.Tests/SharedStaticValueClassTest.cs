namespace Xunit.Isolation.Tests;

public class SharedStaticValueClassTest
{
    public static int StaticValue;

    public class InnerClass1
    {
        [Fact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass2
    {
        [Fact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass3
    {
        [Fact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass4
    {
        [Fact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }
}
