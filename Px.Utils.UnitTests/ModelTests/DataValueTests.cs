using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class DataValueTests
    {
        [TestMethod]
        public void DecimalDataValueSumTest_1_1_2()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(2, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTest_1_0_1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTest_1_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTest_Missing_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest_1_1_2()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(2, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest_1_0_1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest_1_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest_Missing_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }
    }
}
