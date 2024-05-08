﻿using Px.Utils.PxFile.Data;

namespace PxFileTests.DataTests
{
    [TestClass]
    public class DataIndexerTests
    {
        [TestMethod]
        public void OneDimension1NextTest()
        {
            var coordinates = new int[][]
            {
                [ 0 ]
            };
            DataIndexer generator = new(coordinates, [1]);
            long[] expected = [0];
            int i = 0;

            Assert.AreEqual(1, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void OneDimension5NextTest()
        {
            var coordinates = new int[][]
            {
                [ 2, 3, 4 ]
            };
            DataIndexer generator = new(coordinates, [5]);
            long[] expected = [2, 3, 4];
            int i = 0;

            Assert.AreEqual(3, generator.DataLength);

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void OneDimension5LastTest()
        {
            var coordinates = new int[][]
            {
                [ 4 ]
            };
            DataIndexer generator = new(coordinates, [5]);
            long[] expected = [4];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valNextTest()
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

        [TestMethod]
        public void ThreeDimensions3val2val5valReorderingTest()
        {
            var coordinates = new int[][]
            {
                [ 2, 1 ],
                [ 0, 1 ],
                [ 2, 3, 4 ]
            };
            DataIndexer generator = new(coordinates, [3, 2, 5]);
            long[] expected = [22, 23, 24, 27, 28, 29, 12, 13, 14, 17, 18, 19];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valMultidimensionalReorderingTest()
        {
            var coordinates = new int[][]
            {
                [ 2, 1 ],
                [ 0, 1 ],
                [ 4, 3, 2 ]
            };
            DataIndexer generator = new(coordinates, [3, 2, 5]);
            long[] expected = [24, 23, 22, 29, 28, 27, 14, 13, 12, 19, 18, 17];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }

        [TestMethod]
        public void ThreeDimensions3val2val5valMultidimensionalReorderingTestWithDimOrderChange()
        {
            var coordinates = new int[][]
            {
                [ 2, 1 ],
                [ 0, 1 ],
                [ 4, 3, 2 ]
            };
            int[] dimOrder = [1, 0, 2];
            int[] dimSizes = [3, 2, 5];
            DataIndexer generator = new(coordinates, dimOrder, dimSizes);
            long[] expected = [24, 23, 22, 14, 13, 12, 29, 28, 27, 19, 18, 17];
            int i = 0;

            do
            {
                Assert.AreEqual(expected[i++], generator.CurrentIndex);
            }
            while (generator.Next());
        }
    }
}
