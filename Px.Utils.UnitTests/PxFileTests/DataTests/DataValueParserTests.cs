using PxUtils.Models.Data;
using PxUtils.Models.Data.DataValue;
using PxUtils.PxFile.Data;

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
        public void FastParseDoubleDataValueDangerous_ValidInteger_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_ValidZeroInteger_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_ValidNegativeInteger_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_NillSymbol_ReturnsNillValueType()
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
        public void FastParseDoubleDataValueDangerous_ValidWithDecimalPart_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_ValidZeroWithDecimalPart_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_ValidNegativeWithDecimalPart_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_ValidWithDecimalPartAndTrailingZeroes_ReturnsDoubleDataValue()
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
        public void FastParseDoubleDataValueDangerous_MissingSymbol_ReturnsMissingValueType()
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
        public void FastParseDoubleDataValueDangerous_CanNotRepresentSymbol_ReturnsCanNotRepresentValueType()
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
        public void FastParseDoubleDataValueDangerous_ConfidentialSymbol_ReturnsConfidentialValueType()
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
        public void FastParseDoubleDataValueDangerous_NotAcquiredSymbol_ReturnsNotAcquiredValueType()
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
        public void FastParseDoubleDataValueDangerous_NotAskedSymbol_ReturnsNotAskedValueType()
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
        public void FastParseDoubleDataValueDangerous_EmptySymbol_ReturnsEmptyValueType()
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
        public void FastParseDecimalDataValueDangerous_ValidInteger_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_ValidZeroInteger_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_ValidNegativeInteger_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_NillSymbol_ReturnsNillValueType()
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
        public void FastParseDecimalDataValueDangerous_ValidWithDecimalPart_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_ValidZeroWithDecimalPart_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_ValidNegativeWithDecimalPart_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_ValidWithDecimalPartAndTrailingZeroes_ReturnsDecimalDataValue()
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
        public void FastParseDecimalDataValueDangerous_MissingSymbol_ReturnsMissingValueType()
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
        public void FastParseDecimalDataValueDangerous_CanNotRepresentSymbol_ReturnsCanNotRepresentValueType()
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
        public void FastParseDecimalDataValueDangerous_ConfidentialSymbol_ReturnsConfidentialValueType()
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
        public void FastParseDecimalDataValueDangerous_NotAcquiredSymbol_ReturnsNotAcquiredValueType()
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
        public void FastParseDecimalDataValueDangerous_NotAskedSymbol_ReturnsNotAskedValueType()
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
        public void FastParseDecimalDataValueDangerous_EmptySymbol_ReturnsEmptyValueType()
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
        public void FastParseUnsafeDoubleDangerous_ValidInteger_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_ValidZeroInteger_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_ValidNegativeInteger_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_NillSymbol_ReturnsNillValue()
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
        public void FastParseUnsafeDoubleDangerous_ValidWithDecimalPart_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_ValidZeroWithDecimalPart_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_ValidNegativeWithDecimalPart_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_ValidWithDecimalPartAndTrailingZeroes_ReturnsDouble()
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
        public void FastParseUnsafeDoubleDangerous_MissingSymbol_ReturnsMissingValue()
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
        public void FastParseUnsafeDoubleDangerous_CanNotRepresentSymbol_ReturnsCanNotRepresentValue()
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
        public void FastParseUnsafeDoubleDangerous_ConfidentialSymbol_ReturnsConfidentialValue()
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
        public void FastParseUnsafeDoubleDangerous_NotAcquiredSymbol_ReturnsNotAcquiredValue()
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
        public void FastParseUnsafeDoubleDangerous_NotAskedSymbol_ReturnsNotAskedValue()
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
        public void FastParseUnsafeDoubleDangerous_EmptySymbol_ReturnsEmptyValue()
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
        public void ParseDoubleDataValue_ValidInteger_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_ValidZeroInteger_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_ValidNegativeInteger_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_NillSymbol_ReturnsNillValueType()
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
        public void ParseDoubleDataValue_ValidWithDecimalPart_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_ValidZeroWithDecimalPart_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_ValidNegativeWithDecimalPart_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_ValidWithDecimalPartAndTrailingZeroes_ReturnsDoubleDataValue()
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
        public void ParseDoubleDataValue_MissingSymbol_ReturnsMissingValueType()
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
        public void ParseDoubleDataValue_CanNotRepresentSymbol_ReturnsCanNotRepresentValueType()
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
        public void ParseDoubleDataValue_ConfidentialSymbol_ReturnsConfidentialValueType()
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
        public void ParseDoubleDataValue_NotAcquiredSymbol_ReturnsNotAcquiredValueType()
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
        public void ParseDoubleDataValue_NotAskedSymbol_ReturnsNotAskedValueType()
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
        public void ParseDoubleDataValue_EmptySymbol_ReturnsEmptyValueType()
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
        public void ParseDoubleDataValue_MissingCodeWithoutQuotes_Throws()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValue_MissingCodeWithoutEndQuote_Throws()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValue_TwoNegativeSigns_Throws()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValue_TwoDecimalSeparators_Throws()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDoubleDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDoubleDataValue_InvalidCharacters_Throws()
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
        public void ParseDecimalDataValue_ValidInteger_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_ValidZeroInteger_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_ValidNegativeInteger_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_NillSymbol_ReturnsNillValueType()
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
        public void ParseDecimalDataValue_ValidWithDecimalPart_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_ValidZeroWithDecimalPart_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_ValidNegativeWithDecimalPart_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_ValidWithDecimalPartAndTrailingZeroes_ReturnsDecimalDataValue()
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
        public void ParseDecimalDataValue_MissingSymbol_ReturnsMissingValueType()
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
        public void ParseDecimalDataValue_CanNotRepresentSymbol_ReturnsCanNotRepresentValueType()
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
        public void ParseDecimalDataValue_ConfidentialSymbol_ReturnsConfidentialValueType()
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
        public void ParseDecimalDataValue_NotAcquiredSymbol_ReturnsNotAcquiredValueType()
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
        public void ParseDecimalDataValue_NotAskedSymbol_ReturnsNotAskedValueType()
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
        public void ParseDecimalDataValue_EmptySymbol_ReturnsEmptyValueType()
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
        public void ParseDecimalDataValue_MissingCodeWithoutQuotes_Throws()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValue_MissingCodeWithoutEndQuote_Throws()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValue_TwoNegativeSigns_Throws()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValue_TwoDecimalSeparators_Throws()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseDecimalDataValue(buffer, len));
        }

        [TestMethod]
        public void ParseDecimalDataValue_InvalidCharacters_Throws()
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
        public void ParseUnsafeDouble_ValidInteger_ReturnsDouble()
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
        public void ParseUnsafeDouble_ValidZeroInteger_ReturnsDouble()
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
        public void ParseUnsafeDouble_ValidNegativeInteger_ReturnsDouble()
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
        public void ParseUnsafeDouble_NillSymbol_ReturnsNillValue()
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
        public void ParseUnsafeDouble_ValidWithDecimalPart_ReturnsDouble()
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
        public void ParseUnsafeDouble_ValidZeroWithDecimalPart_ReturnsDouble()
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
        public void ParseUnsafeDouble_ValidNegativeWithDecimalPart_ReturnsDouble()
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
        public void ParseUnsafeDouble_ValidWithDecimalPartAndTrailingZeroes_ReturnsDouble()
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
        public void ParseUnsafeDouble_MissingSymbol_ReturnsMissingValue()
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
        public void ParseUnsafeDouble_CanNotRepresentSymbol_ReturnsCanNotRepresentValue()
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
        public void ParseUnsafeDouble_ConfidentialSymbol_ReturnsConfidentialValue()
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
        public void ParseUnsafeDouble_NotAcquiredSymbol_ReturnsNotAcquiredValue()
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
        public void ParseUnsafeDouble_NotAskedSymbol_ReturnsNotAskedValue()
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
        public void ParseUnsafeDouble_EmptySymbol_ReturnsEmptyValue()
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
        public void ParseUnsafeDouble_MissingCodeWithoutQuotes_Throws()
        {
            // Arrange
            char[] buffer = ['.', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDouble_MissingCodeWithoutEndQuote_Throws()
        {
            // Arrange
            char[] buffer = ['"', '.', '.', '.',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDouble_TwoNegativeSigns_Throws()
        {
            // Arrange
            char[] buffer = ['-', '-', '3', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDouble_TwoDecimalSeparators_Throws()
        {
            // Arrange
            char[] buffer = ['3', '.', '.', '2',];
            int len = 4;

            // Act
            Assert.ThrowsException<ArgumentException>(() => DataValueParsers.ParseUnsafeDouble(buffer, len, missingValueEncodings));
        }

        [TestMethod]
        public void ParseUnsafeDouble_InvalidCharacters_Throws()
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
