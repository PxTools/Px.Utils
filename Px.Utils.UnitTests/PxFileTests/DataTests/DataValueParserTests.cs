using Px.Utils.Models.Data;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.PxFile.Data;

namespace PxFileTests.DataTests
{
    [TestClass]
    public class DataValueParserTests
    {
        #region FastParseDoubleDataValueDangerous

        /* 
         * FastParseDoubleDataValueDangerous() is only meant to parse PREVALIDATED inputs.
         * Invalid inputs cause undefined behaviour by design and are therefore not tested here
        */

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(123456789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidZeroIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidNegativeIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(-123456789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousNillSymbolReturnsNillValueType()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Nill, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(1234.56789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidZeroWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0.0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidNegativeWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(-1234.56789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousValidWithDecimalPartAndTrailingZeroesReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(1234.56, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousMissingSymbolReturnsMissingValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Missing, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousCanNotRepresentSymbolReturnsCanNotRepresentValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.CanNotRepresent, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousConfidentialSymbolReturnsConfidentialValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Confidential, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousNotAcquiredSymbolReturnsNotAcquiredValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAcquired, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousNotAskedSymbolReturnsNotAskedValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAsked, result.Type);
        }

        [TestMethod]
        public void FastParseDoubleDataValueDangerousEmptySymbolReturnsEmptyValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            DoubleDataValue result = DataValueParsers.FastParseDoubleDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Empty, result.Type);
        }

        #endregion

        #region FastParseDecimalDataValueDangerous

        /* 
         * FastParseDecimalDataValueDangerous() is only meant to parse PREVALIDATED inputs.
         * Invalid inputs cause undefined behaviour by design and are therefore not tested here
        */

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(123456789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidZeroIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidNegativeIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(-123456789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousNillSymbolReturnsNillValueType()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Nill, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(1234.56789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidZeroWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(0.0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidNegativeWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(-1234.56789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousValidWithDecimalPartAndTrailingZeroesReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(1234.56m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousMissingSymbolReturnsMissingValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Missing, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousCanNotRepresentSymbolReturnsCanNotRepresentValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.CanNotRepresent, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousConfidentialSymbolReturnsConfidentialValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Confidential, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousNotAcquiredSymbolReturnsNotAcquiredValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAcquired, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousNotAskedSymbolReturnsNotAskedValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAsked, result.Type);
        }

        [TestMethod]
        public void FastParseDecimalDataValueDangerousEmptySymbolReturnsEmptyValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            DecimalDataValue result = DataValueParsers.FastParseDecimalDataValueDangerous(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Empty, result.Type);
        }

        #endregion

        #region FastParseUnsafeDoubleDangerous

        /* 
         * FastParseUnsafeDoubleDangerous() is only meant to parse PREVALIDATED inputs.
         * Invalid inputs cause undefined behaviour by design and are therefore not tested here
        */

        private readonly double[] missingValueEncodings = [ 0, 1, 2, 3, 4, 5, 6 ];

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(123456789, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidZeroIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidNegativeIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(-123456789, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousNillSymbolReturnsNillValue()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1234.56789, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidZeroWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidNegativeWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(-1234.56789, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousValidWithDecimalPartAndTrailingZeroesReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1234.56, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousMissingSymbolReturnsMissingValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousCanNotRepresentSymbolReturnsCanNotRepresentValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousConfidentialSymbolReturnsConfidentialValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousNotAcquiredSymbolReturnsNotAcquiredValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousNotAskedSymbolReturnsNotAskedValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void FastParseUnsafeDoubleDangerousEmptySymbolReturnsEmptyValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            double result = DataValueParsers.FastParseUnsafeDoubleDangerous(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(6, result);
        }

        #endregion

        #region ParseDoubleDataValue

        [TestMethod]
        public void ParseDoubleDataValueValidIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(123456789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidZeroIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidNegativeIntegerReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(-123456789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueNillSymbolReturnsNillValueType()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Nill, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(1234.56789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidZeroWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0.0, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidNegativeWithDecimalPartReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(-1234.56789, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueValidWithDecimalPartAndTrailingZeroesReturnsDoubleDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(1234.56, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueMissingSymbolReturnsMissingValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Missing, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueCanNotRepresentSymbolReturnsCanNotRepresentValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.CanNotRepresent, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueConfidentialSymbolReturnsConfidentialValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Confidential, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueNotAcquiredSymbolReturnsNotAcquiredValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAcquired, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueNotAskedSymbolReturnsNotAskedValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAsked, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueEmptySymbolReturnsEmptyValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            DoubleDataValue result = DataValueParsers.ParseDoubleDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Empty, result.Type);
        }

        [TestMethod]
        public void ParseDoubleDataValueMissingCodeWithoutQuotesThrows()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValueMissingCodeWithoutEndQuoteThrows()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValueTwoNegativeSignsThrows()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValueTwoDecimalSeparatorsThrows()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValueInvalidCharactersThrows()
        {
            // Arrange
            char[] buffer = ['3', 'r', 'z', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        #endregion

        #region ParseDecimalDataValue

        [TestMethod]
        public void ParseDecimalDataValueValidIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(123456789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidZeroIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidNegativeIntegerReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(-123456789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueNillSymbolReturnsNillValueType()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Nill, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(1234.56789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidZeroWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(0.0m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidNegativeWithDecimalPartReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(-1234.56789m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueValidWithDecimalPartAndTrailingZeroesReturnsDecimalDataValue()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(1234.56m, result.UnsafeValue);
            Assert.AreEqual(DataValueType.Exists, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueMissingSymbolReturnsMissingValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Missing, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueCanNotRepresentSymbolReturnsCanNotRepresentValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.CanNotRepresent, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueConfidentialSymbolReturnsConfidentialValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Confidential, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueNotAcquiredSymbolReturnsNotAcquiredValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAcquired, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueNotAskedSymbolReturnsNotAskedValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.NotAsked, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueEmptySymbolReturnsEmptyValueType()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            DecimalDataValue result = DataValueParsers.ParseDecimalDataValue(buffer, len);

            // Assert
            Assert.AreEqual(DataValueType.Empty, result.Type);
        }

        [TestMethod]
        public void ParseDecimalDataValueMissingCodeWithoutQuotesThrows()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValueMissingCodeWithoutEndQuoteThrows()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValueTwoNegativeSignsThrows()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValueTwoDecimalSeparatorsThrows()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValueInvalidCharactersThrows()
        {
            // Arrange
            char[] buffer = ['3', 'r', 'z', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        #endregion

        #region ParseUnsafeDouble
        
        [TestMethod]
        public void ParseUnsafeDoubleValidIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 9;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(123456789, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidZeroIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['0'];
            int len = 1;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidNegativeIntegerReturnsDouble()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(-123456789, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleNillSymbolReturnsNillValue()
        {
            // Arrange
            char[] buffer = ['"', '-', '"'];
            int len = 3;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 10;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1234.56789, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidZeroWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['0', '.', '0', '0'];
            int len = 4;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidNegativeWithDecimalPartReturnsDouble()
        {
            // Arrange
            char[] buffer = ['-', '1', '2', '3', '4', '.', '5', '6', '7', '8', '9'];
            int len = 11;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(-1234.56789, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleValidWithDecimalPartAndTrailingZeroesReturnsDouble()
        {
            // Arrange
            char[] buffer = ['1', '2', '3', '4', '.', '5', '6', '0', '0', '0'];
            int len = 10;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1234.56, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleMissingSymbolReturnsMissingValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '"'];
            int len = 3;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleCanNotRepresentSymbolReturnsCanNotRepresentValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '"'];
            int len = 4;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleConfidentialSymbolReturnsConfidentialValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '"'];
            int len = 5;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleNotAcquiredSymbolReturnsNotAcquiredValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '"'];
            int len = 6;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleNotAskedSymbolReturnsNotAskedValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '"'];
            int len = 7;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleEmptySymbolReturnsEmptyValue()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.', '.', '.', '.', '"'];
            int len = 8;

            // Act
            double result = DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings);

            // Assert
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void ParseUnsafeDoubleMissingCodeWithoutQuotesThrows()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDoubleMissingCodeWithoutEndQuoteThrows()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDoubleTwoNegativeSignsThrows()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDoubleTwoDecimalSeparatorsThrows()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDoubleInvalidCharactersThrows()
        {
            // Arrange
            char[] buffer = ['3', 'r', 'z', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        #endregion
    }
}
