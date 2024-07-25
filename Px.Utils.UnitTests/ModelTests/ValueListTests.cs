using Px.Utils.Language;
using Px.Utils.Models.Metadata.Dimensions;

namespace Px.Utils.UnitTests.ModelTests
{
    [TestClass]
    public class ValueListTests
    {
        [TestMethod]
        public void ValueListMapReturnsPropertiesOfDimensionValues()
        {
            // Arrange
            List<DimensionValue> values =
            [
                new ("1", new ("en", "foo"), true),
                new ("2", new ("en", "bar"), false),
            ];
            ValueList valueList = new (values);

            // Act
            string[] codes = valueList.Map(v => v.Code).ToArray();
            string[] names = valueList.Map(v => v.Name["en"]).ToArray();
            bool[] isVirtual = valueList.Map(v => v.Virtual).ToArray();

            // Assert
            Assert.AreEqual(2, codes.Length);
            Assert.AreEqual("1", codes[0]);
            Assert.AreEqual("2", codes[1]);

            Assert.AreEqual(2, names.Length);
            Assert.AreEqual("foo", names[0]);
            Assert.AreEqual("bar", names[1]);

            Assert.AreEqual(2, isVirtual.Length);
            Assert.IsTrue(isVirtual[0]);
            Assert.IsFalse(isVirtual[1]);
        }

        [TestMethod]
        public void ContentValueListMapReturnsPropertiesOfContentDimensionValues()
        {
            // Arrange
            List<ContentDimensionValue> values =
            [
                new("1", new("en", "foo"), new("en", "kg"), DateTime.MinValue, 0),
                new("2", new("en", "bar"), new("en", "m"), DateTime.MaxValue, 1)
            ];
            ContentValueList valueList = new(values);

            // Act
            string[] codes = valueList.Map(v => v.Code).ToArray();
            string[] names = valueList.Map(v => v.Name["en"]).ToArray();
            string[] units = valueList.Map(v => v.Unit["en"]).ToArray();
            DateTime[] lastUpdated = valueList.Map(v => v.LastUpdated).ToArray();
            int[] precisions = valueList.Map(v => v.Precision).ToArray();

            // Assert
            Assert.AreEqual(2, codes.Length);
            Assert.AreEqual("1", codes[0]);
            Assert.AreEqual("2", codes[1]);

            Assert.AreEqual(2, names.Length);
            Assert.AreEqual("foo", names[0]);
            Assert.AreEqual("bar", names[1]);

            Assert.AreEqual(2, units.Length);
            Assert.AreEqual("kg", units[0]);
            Assert.AreEqual("m", units[1]);

            Assert.AreEqual(2, lastUpdated.Length);
            Assert.AreEqual(DateTime.MinValue, lastUpdated[0]);
            Assert.AreEqual(DateTime.MaxValue, lastUpdated[1]);

            Assert.AreEqual(2, precisions.Length);
            Assert.AreEqual(0, precisions[0]);
            Assert.AreEqual(1, precisions[1]);
        }
    }
}
