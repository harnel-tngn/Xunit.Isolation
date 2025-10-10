namespace Xunit.Isolation.Tests;

public class SharedStaticValueClassTest
{
    public static int StaticValue;

    public abstract class InnerClass
    {
        [Fact]
        public void Test()
        {
            SharedStaticValueClassTest.StaticValue++;
            Assert.Equal(1, SharedStaticValueClassTest.StaticValue);
        }
    }

    public class InnerClass1 : InnerClass;
    public class InnerClass2 : InnerClass;
    public class InnerClass3 : InnerClass;
    public class InnerClass4 : InnerClass;
}
