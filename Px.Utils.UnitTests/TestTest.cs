namespace Px.Utils.UnitTests
{
    public class TestTest
    {
        [Fact]
        public void TestTest1()
        {
            InitTest initTest = new();
            Assert.Equal("Hello World", initTest.Test());
        }
    }
}