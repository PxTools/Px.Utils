using Px.Utils.Models.Metadata;
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
        private readonly int[] _dimOrder;
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
            _dimOrder = GetDimensionOrder(completeMetaMap, targetMap);
            for (int targetIndex = 0; targetIndex < targetMap.DimensionMaps.Count; targetIndex++)
            {
                int dimIndex = _dimOrder[targetIndex];
                IDimensionMap dimension = completeMetaMap.DimensionMaps.First(d => d.Code == targetMap.DimensionMaps[targetIndex].Code);
                _coordinates[dimIndex] = new int[targetMap.DimensionMaps[targetIndex].ValueCodes.Count];
                for (int mapIndex = 0; mapIndex < targetMap.DimensionMaps[targetIndex].ValueCodes.Count; mapIndex++)
                {
                    for (int valIndex = 0; valIndex < dimension.ValueCodes.Count; valIndex++)
                    {
                        if (dimension.ValueCodes[valIndex] == targetMap.DimensionMaps[targetIndex].ValueCodes[mapIndex])
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
            _dimOrder = Enumerable.Range(0, _coordinates.Length).ToArray();
            DataLength = _coordinates.Select(c => c.Length).Aggregate(1, (a, b) => a * b);
            _indices = new int[_coordinates.Length];
            _lastIndices = _coordinates.Select(d => d.Length - 1).ToArray();
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
        /// <param name="dimensionOrder">
        /// If the order of the dimensions in the original set is different from the order of the dimensions in the coordinates, this list can be used to specify the order.
        /// [0] = 1 means that the first dimension in the coordinates corresponds to the second dimension in the original set.
        /// </param>
        /// <param name="dimensionSizes">
        /// Sizes of the dimensions in the original set.
        /// </param>
        public DataIndexer(int[][] coordinates, int[] dimensionOrder, int[] dimensionSizes)
        {
            _coordinates = [.. coordinates];
            _dimOrder = [.. dimensionOrder];
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
                int dimIndex = _dimOrder[i];
                if (_indices[dimIndex] < _lastIndices[dimIndex])
                {
                    _indices[dimIndex]++;
                    SetCurrentIndex();
                    return true;
                }
                else _indices[dimIndex] = 0;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetCurrentIndex()
        {
            CurrentIndex = 0;
            for (int i = 0; i <= _lastCoordinateIndex; i++)
            {
                int dimIndex = _dimOrder[i];
                CurrentIndex += _ReverseCumulativeProducts[dimIndex] * _coordinates[dimIndex][_indices[dimIndex]];
            }
        }

        private static int[] GetDimensionOrder(IMatrixMap completeMetaMap, IMatrixMap targetMap)
        {
            string[] sourceCodes = completeMetaMap.DimensionMaps.Select(d => d.Code).ToArray();
            return targetMap.DimensionMaps.Select(d => Array.IndexOf(sourceCodes, d.Code)).ToArray();
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
