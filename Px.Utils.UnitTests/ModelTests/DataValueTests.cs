using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class DataValueTests
    {
        #region Sum

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

        #endregion

        #region Subtraction

        [TestMethod]
        public void DecimalDataValueSubTest_1_1_0()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTest_1_0_1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTest_1_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTest_Missing_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest_1_1_0()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest_1_0_1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest_1_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest_Missing_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        #endregion

        #region Multiplication

        [TestMethod]
        public void DecimalDataValueMulTest_1_1_1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTest_1_0_0()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(0, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTest_1_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTest_Missing_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTest_1_1_1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest_1_0_0()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(0, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTest_1_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTest_Missing_Missing_Missing()
        {
            DoubleDataValue a = new(0, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        #endregion

        #region Division

        [TestMethod]
        public void DecimalDataValueDivTest_1_1_1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTest_1_0_0()
        {
            DecimalDataValue a = new(0, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTest_0_1_Throws()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.ThrowsException<DivideByZeroException>(() => a / b);
        }

        [TestMethod]
        public void DecimalDataValueDivTest_1_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTest_Missing_Missing_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest_1_1_1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest_0_1_0()
        {
            DoubleDataValue a = new(0, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest_1_0_Throws()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.ThrowsException<DivideByZeroException>(() => a / b);
        }

        [TestMethod]
        public void DoubleDataValueDivTest_1_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest_Missing_Missing_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        #endregion

        #region Unary

        [TestMethod]
        public void DecimalDataValueUnaryPlusTest()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            a = +a;

            Assert.AreEqual(1, a.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, a.Type);
        }

        [TestMethod]
        public void DecimalDataValueUnaryPlusTest_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            a = +a;

            Assert.AreEqual(DataValueType.Missing, a.Type);
        }

        [TestMethod]
        public void DoubleDataValueUnaryPlusTest()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            a = +a;

            Assert.AreEqual(1, a.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, a.Type);
        }

        [TestMethod]
        public void DoubleDataValueUnaryPlusTest_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            a = +a;

            Assert.AreEqual(DataValueType.Missing, a.Type);
        }

        [TestMethod]
        public void DecimalDataValueUnaryMinusTest()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            a = -a;

            Assert.AreEqual(-1, a.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, a.Type);
        }

        [TestMethod]
        public void DecimalDataValueUnaryMinusTest_Missing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            a = -a;

            Assert.AreEqual(DataValueType.Missing, a.Type);
        }

        [TestMethod]
        public void DoubleDataValueUnaryMinusTest()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            a = -a;

            Assert.AreEqual(-1, a.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, a.Type);
        }

        [TestMethod]
        public void DoubleDataValueUnaryMinusTest_Missing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            a = -a;

            Assert.AreEqual(DataValueType.Missing, a.Type);
        }

        #endregion
    }
}
