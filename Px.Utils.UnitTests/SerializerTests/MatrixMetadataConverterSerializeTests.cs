using Px.Utils.Models.Metadata;
using Px.Utils.UnitTests.SerializerTests.Fixtures.Json;
using Px.Utils.UnitTests.SerializerTests.Fixtures.Objects;
using System.Text.Json;

namespace Px.Utils.UnitTests.SerializerTests
{
    [TestClass]
    public class MatrixMetadataConverterSerializeTests
    {
        private static readonly JsonSerializerOptions IndentedCachedOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions OneLineCachedOptions = new()
        {
            WriteIndented = false
        };

        [TestMethod]
        public void SerializeMetadataFromInterfaceMatchesFixture()
        {
            IReadOnlyMatrixMetadata metadata = MatrixMetadataFixture.SimpleMeta;
            string output = JsonSerializer.Serialize(metadata, IndentedCachedOptions);
            Assert.AreEqual(MatrixMetadataJson.SimpleMeta, output);
        }

        [TestMethod]
        public void SerializeMetadataMatchesFixture()
        {
            string output = JsonSerializer.Serialize(MatrixMetadataFixture.SimpleMeta, IndentedCachedOptions);
            Assert.AreEqual(MatrixMetadataJson.SimpleMeta, output);
        }

        [TestMethod]
        public void DeserializationAndSerializationShouldNotChangeOutput()
        {
            string serialized = JsonSerializer.Serialize(MatrixMetadataFixture.SimpleMeta, IndentedCachedOptions);
            MatrixMetadata? deserialized = JsonSerializer.Deserialize<MatrixMetadata>(serialized, IndentedCachedOptions);
            string output = JsonSerializer.Serialize(deserialized, IndentedCachedOptions);
            Assert.AreEqual(serialized, output);
        }

        [TestMethod]
        public void SerializeMetadataFromInterfaceToOneLineMatchesFixture()
        {
            IReadOnlyMatrixMetadata metadata = MatrixMetadataFixture.SimpleMeta;
            string output = JsonSerializer.Serialize(metadata, OneLineCachedOptions);
            Assert.AreEqual(MatrixMetadataJson.SimpleMetaWithoutWhitespace, output);
        }

        [TestMethod]
        public void SerializeMetadataToOneLineMatchesFixture()
        {
            string output = JsonSerializer.Serialize(MatrixMetadataFixture.SimpleMeta, OneLineCachedOptions);
            Assert.AreEqual(MatrixMetadataJson.SimpleMetaWithoutWhitespace, output);
        }
    }
}
