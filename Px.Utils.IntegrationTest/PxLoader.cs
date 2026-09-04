using Px.Utils.ModelBuilders;
using Px.Utils.Models;
using Px.Utils.Models.Data.DataValue;
using Px.Utils.Models.Metadata;
using Px.Utils.Models.Metadata.ExtensionMethods;
using Px.Utils.PxFile.Data;
using Px.Utils.PxFile.Metadata;
using System.Text;

namespace Px.Utils.IntegrationTest;

internal sealed class PxLoader
{
    public static LoadedMatrix Load(string filePath, IMatrixMap? targetMap = null)
    {
        PxFileMetadataReader metadataReader = new();
        Encoding encoding;
        using (FileStream encodingStream = File.OpenRead(filePath))
        {
            encoding = metadataReader.GetEncoding(encodingStream);
        }

        MatrixMetadata metadata;
        using (FileStream metadataStream = File.OpenRead(filePath))
        {
            MatrixMetadataBuilder metadataBuilder = new();
            metadata = metadataBuilder.Build(metadataReader.ReadMetadata(metadataStream, encoding));
        }

        MatrixMap completeMap = new(metadata.Dimensions.Cast<IDimensionMap>().ToList());
        IMatrixMap requestedMap = targetMap ?? completeMap;
        long size = requestedMap.GetSize();
        DoubleDataValue[] values = new DoubleDataValue[checked((int)size)];
        using (FileStream dataStream = File.OpenRead(filePath))
        using (PxFileStreamDataReader dataReader = new(dataStream))
        {
            dataReader.ReadDoubleDataValues(values, 0, requestedMap, completeMap);
        }

        MatrixMetadata resultMetadata = targetMap is null ? metadata : metadata.GetTransform(requestedMap);
        return new LoadedMatrix(resultMetadata, new Matrix<DoubleDataValue>(resultMetadata, values));
    }

    public static MatrixMap CreateMap(IEnumerable<ExpectedResults.DimensionExpectation> dimensions)
    {
        List<IDimensionMap> maps = [.. dimensions.Select(dimension => (IDimensionMap)new DimensionMap(dimension.Code, [.. dimension.ValueCodes]))];
        return new MatrixMap(maps);
    }
}

internal sealed record LoadedMatrix(MatrixMetadata Metadata, Matrix<DoubleDataValue> Matrix);
