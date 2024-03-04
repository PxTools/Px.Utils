using Px.Utils.UnitTests.ModelBuilderTests.Fixtures;
using PxUtils.ModelBuilders;
using PxUtils.Models.Metadata;

namespace ModelBuilderTests
{
    [TestClass]
    public class MatrixMetadataBuilderTests
    {
        [TestMethod]
        public void BuildTest()
        {
            IEnumerable<KeyValuePair<string, string>> input = PxFileMetaEntries_Robust_3_Languages.Entries;
            MatrixMetadataBuilder builder = new();

            MatrixMetadata actual = builder.Build(input);
            Assert.IsNotNull(actual);
        }
    }
}
