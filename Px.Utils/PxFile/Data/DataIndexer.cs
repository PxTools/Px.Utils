using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using System.Runtime.CompilerServices;

namespace Px.Utils.PxFile.Data
{
    public sealed class DataIndexer
    {
        public long CurrentIndex { get; private set; }
        public int DataLength { get; private set; }

        private readonly int[][] _coordinates;
        private readonly int[] _lastIndices;
        private readonly int[] _indices;
        private readonly int _lastCoordinateIndex;
        private readonly int[] _ReverseCumulativeProducts;

        public DataIndexer(IReadOnlyMatrixMetadata meta, MatrixMap map)
        {
            int[] dimensionSizes = meta.Dimensions.Select(d => d.Values.Count).ToArray();
            _coordinates = new int[map.DimensionMaps.Count][];
            for (int dimIndex = 0; dimIndex < map.DimensionMaps.Count; dimIndex++)
            {
                IReadOnlyDimension dimension = meta.Dimensions.First(d => d.Code == map.DimensionMaps[dimIndex].Code);
                _coordinates[dimIndex] = new int[map.DimensionMaps[dimIndex].ValueCodes.Count];
                for (int mapIndex = 0; mapIndex < map.DimensionMaps[dimIndex].ValueCodes.Count; mapIndex++)
                {
                    for (int valIndex = 0; valIndex < dimension.Values.Count; valIndex++)
                    {
                        if (dimension.Values[valIndex].Code == map.DimensionMaps[dimIndex].ValueCodes[mapIndex])
                        {
                            _coordinates[dimIndex][mapIndex] = valIndex;
                        }
                    }
                }
            }

            DataLength = map.DimensionMaps.Select(dm => dm.ValueCodes.Count).Aggregate(1, (a, b) => a * b);
            _indices = new int[meta.Dimensions.Count];
            _lastIndices = map.DimensionMaps.Select(d => d.ValueCodes.Count - 1).ToArray();
            _lastCoordinateIndex = _coordinates.Length - 1;
            _ReverseCumulativeProducts = GenerateRCP(dimensionSizes);
            SetCurrentIndex();
        }

        public DataIndexer(int[][] coordinates, int[] dimensionSizes)
        {
            _coordinates = [.. coordinates];
            DataLength = _coordinates.Select(c => c.Length).Aggregate(1, (a, b) => a * b);
            _indices = new int[_coordinates.Length];
            _lastIndices = _coordinates.Select(d => d.Length - 1).ToArray();
            _lastCoordinateIndex = _coordinates.Length - 1;
            _ReverseCumulativeProducts = GenerateRCP(dimensionSizes);
            SetCurrentIndex();
        }

        public bool Next()
        {
            for (int i = _lastCoordinateIndex; i >= 0; i--)
            {
                if (_indices[i] < _lastIndices[i])
                {
                    _indices[i]++;
                    SetCurrentIndex();
                    return true;
                }
                else _indices[i] = 0;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCurrentIndex()
        {
            CurrentIndex = 0;
            for (int i = 0; i <= _lastCoordinateIndex; i++)
            {
                CurrentIndex += _ReverseCumulativeProducts[i] * _coordinates[i][_indices[i]];
            }
        }

        private static int[] GenerateRCP(int[] dimensionSizes)
        {
            int numOfDims = dimensionSizes.Length;
            int[] cnt = new int[numOfDims];
            var cumulativeMultiplier = 1;
            for (var i = numOfDims - 1; i >= 0; i--)
            {
                cnt[i] = cumulativeMultiplier;
                cumulativeMultiplier *= dimensionSizes[i];
            }
            return cnt;
        }
    }
}
