using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class DataValueTests
    {
        #region Sum

        [TestMethod]
        public void DecimalDataValueSumTest1Plus1Equals2()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(2, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTest1Plus0Equals1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTest1PlusMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSumTestMissingPlusMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest1Plus1Equals2()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(2, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest1Plus0Equals1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a + b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest1PlusMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTestMissingPlusMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a + b).Type);
        }

        #endregion

        #region Subtraction

        [TestMethod]
        public void DecimalDataValueSubTest1Minus1Equals0()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTest1Minus0Equals1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTest1MinusMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DecimalDataValueSubTestMissingMinusMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest1Minus1Equals0()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest1Minus0Equals1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(1, (a - b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTest1MinusMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSubTestMissingMinusMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a - b).Type);
        }

        #endregion

        #region Multiplication

        [TestMethod]
        public void DecimalDataValueMulTest1MultipliedBy1Equals1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTest1MultipliedBy0Equals0()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(0, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTest1MultipliedByMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DecimalDataValueMulTestMissingMultipliedByMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTest1MultipliedBy1Equals1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueSumTest1MultipliedBy0Equals0()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.AreEqual(0, (a * b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTest1MultipliedByMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        [TestMethod]
        public void DoubleDataValueMulTestMissingMultipliedByMissingEqualsMissing()
        {
            DoubleDataValue a = new(0, DataValueType.Missing);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a * b).Type);
        }

        #endregion

        #region Division

        [TestMethod]
        public void DecimalDataValueDivTest1DividedBy1Equals1()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTest1DividedBy0Equals0()
        {
            DecimalDataValue a = new(0, DataValueType.Exists);
            DecimalDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTest0DividedBy1Throws()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Exists);

            Assert.ThrowsException<DivideByZeroException>(() => a / b);
        }

        [TestMethod]
        public void DecimalDataValueDivTest1DividedByMissingDividedByMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Exists);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DecimalDataValueDivTestMissingDividedByMissingEqualsMissing()
        {
            DecimalDataValue a = new(1, DataValueType.Missing);
            DecimalDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest1DividedBy1Equals1()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(1, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest0DividedBy1Equals0()
        {
            DoubleDataValue a = new(0, DataValueType.Exists);
            DoubleDataValue b = new(1, DataValueType.Exists);

            Assert.AreEqual(0, (a / b).UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTest1DividedBy0Throws()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Exists);

            Assert.ThrowsException<DivideByZeroException>(() => a / b);
        }

        [TestMethod]
        public void DoubleDataValueDivTest1DividedByMissingEqualsMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Exists);
            DoubleDataValue b = new(0, DataValueType.Missing);

            Assert.AreEqual(DataValueType.Missing, (a / b).Type);
        }

        [TestMethod]
        public void DoubleDataValueDivTestMissingDividedByMissingEqualsMissing()
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
        public void DecimalDataValueUnaryPlusTestMissing()
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
        public void DoubleDataValueUnaryPlusTestMissing()
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
        public void DecimalDataValueUnaryMinusTestMissing()
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
        public void DoubleDataValueUnaryMinusTestMissing()
        {
            DoubleDataValue a = new(1, DataValueType.Missing);
            a = -a;

            Assert.AreEqual(DataValueType.Missing, a.Type);
        }

        #endregion
    }
}
