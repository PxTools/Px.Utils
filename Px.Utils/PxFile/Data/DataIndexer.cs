using Px.Utils.Models.Metadata;
using PxUtils.Models.Metadata;
using PxUtils.Models.Metadata.Dimensions;
using System.Runtime.CompilerServices;

namespace Px.Utils.PxFile.Data
{
    public sealed class DataIndexer
    {
        /// <summary>
        /// Current index.
        /// </summary>
        public long CurrentIndex { get; private set; }

        /// <summary>
        /// The number of items that can be indexed.
        /// </summary>
        public int DataLength { get; private set; }

        private readonly int[][] _coordinates;
        private readonly int[] _lastIndices;
        private readonly int[] _indices;
        private readonly int _lastCoordinateIndex;
        private readonly int[] _ReverseCumulativeProducts;

        /// <summary>
        /// Builds an indexer based on a complete metadata map of the original set and a target map.
        /// </summary>
        /// <param name="completeMetaMap">Metadata map covering the whole starting set on which the indexing is based on.</param>
        /// <param name="targetMap">Produces the indexes of the items decribed by this map.</param>
        public DataIndexer(IMatrixMap completeMetaMap, IMatrixMap targetMap)
        {
            int[] dimensionSizes = completeMetaMap.DimensionMaps.Select(d => d.ValueCodes.Count).ToArray();
            _coordinates = new int[targetMap.DimensionMaps.Count][];
            for (int dimIndex = 0; dimIndex < targetMap.DimensionMaps.Count; dimIndex++)
            {
                IDimensionMap dimension = completeMetaMap.DimensionMaps.First(d => d.Code == targetMap.DimensionMaps[dimIndex].Code);
                _coordinates[dimIndex] = new int[targetMap.DimensionMaps[dimIndex].ValueCodes.Count];
                for (int mapIndex = 0; mapIndex < targetMap.DimensionMaps[dimIndex].ValueCodes.Count; mapIndex++)
                {
                    for (int valIndex = 0; valIndex < dimension.ValueCodes.Count; valIndex++)
                    {
                        if (dimension.ValueCodes[valIndex] == targetMap.DimensionMaps[dimIndex].ValueCodes[mapIndex])
                        {
                            _coordinates[dimIndex][mapIndex] = valIndex;
                        }
                    }
                }
            }

            DataLength = targetMap.DimensionMaps.Select(dm => dm.ValueCodes.Count).Aggregate(1, (a, b) => a * b);
            _indices = new int[completeMetaMap.DimensionMaps.Count];
            _lastIndices = targetMap.DimensionMaps.Select(d => d.ValueCodes.Count - 1).ToArray();
            _lastCoordinateIndex = _coordinates.Length - 1;
            _ReverseCumulativeProducts = GenerateRCP(dimensionSizes);
            SetCurrentIndex();
        }

        /// <summary>
        /// Builds an indexer based on a set of coordinates and the sizes of the dimensions in the original set.
        /// </summary>
        /// <param name="coordinates">
        /// Produces the indexes of the items in these coordinates.
        /// Each sublist in the list represents a dimension and the values in the sublist are the indexes of the values in the original set.
        /// </param>
        /// <param name="dimensionSizes">
        /// Sizes of the dimensions in the original set.
        /// </param>
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

        /// <summary>
        /// Moves the indexer to the next item in the target map if possible.
        /// </summary>
        /// <returns>True if the indexer was moved to the next item, false if the indexer is at the last item.</returns>
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
