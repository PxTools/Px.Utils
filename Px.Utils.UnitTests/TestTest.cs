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

        /// <summary>
        /// This test should fail. It is used to test the CI/CD pipeline and will be removed when the testing is done.
        /// </summary>
        [Fact]
        public void FailTest1()
        {
            Assert.True(false);
        }
    }
}