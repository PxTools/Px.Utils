using Px.Utils.PxFile.Data;

namespace PxFileTests.DataTests
{
    [TestClass]
    public class ReadIndexGeneratorTests
    {
        [TestMethod]
        public void NextTest()
        {
            var coordinates = new int[][]
            {
                [ 1, 2 ],
                [ 0, 1 ],
                [ 2, 3, 4 ]
            };
            DataIndexer generator = new(coordinates, [3, 2, 5]);
            long[] expected = [12, 13, 14, 17, 18, 19, 22, 23, 24, 27, 28, 29];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            } 
            while (generator.Next());
        }
    }
}
