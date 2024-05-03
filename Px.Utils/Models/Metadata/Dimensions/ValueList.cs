using PxUtils.Models.Metadata.Dimensions;
using System.Collections;

namespace Px.Utils.Models.Metadata.Dimensions
{
    /*
     * This list wrapper class is used to enable different types of dimension values inherited from
     * DimensionValue class to be used with different dimension types inherited from the Dimension base class.
     * Otherwise, the items in the list would be of the base class type and require casting.
     */

    /// <summary>
    /// Ordered collection of dimension values with utility methods.
    /// </summary>
    public class ValueList : IReadOnlyList<IReadOnlyDimensionValue>
    {
        protected List<DimensionValue> Dimensions { get; }

        /// <summary>
        /// Gets the dimension value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the dimension value to get.</param>
        /// <returns>The dimension value at the specified index.</returns>
        public virtual DimensionValue this[int index] => Dimensions[index];

        IReadOnlyDimensionValue IReadOnlyList<IReadOnlyDimensionValue>.this[int index] => Dimensions[index];

        /// <summary>
        /// Gets the number of dimension values in the list.
        /// </summary>
        public int Count => Dimensions.Count;

        /// <summary>
        /// Gets the list of codes associated with the dimension values in the same order.
        /// </summary>
        public IReadOnlyList<string> Codes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueList"/> class.
        /// </summary>
        /// <param name="dimensions">The collection of dimension values in order.</param>
        public ValueList(IEnumerable<DimensionValue> dimensions)
        {
            Dimensions = dimensions.ToList();
            Codes = Dimensions.Select(d => d.Code).ToList();
        }

        /// <summary>
        /// Finds the dimension value with the specified code.
        /// </summary>
        /// <param name="code">The code of the dimension value to find.</param>
        /// <returns>The dimension value with the specified code, or null if not found.</returns>
        public virtual DimensionValue? Find(string code) => Dimensions.Find(d => d.Code == code);

        /// <summary>
        /// Returns the first dimension value that fills the specified condition.
        /// </summary>
        /// <param name="match">The condition the check the dimension value against.</param>
        /// <returns>The first dimension value that matches the specified predicate, or null if not found.</returns>
        public DimensionValue? Find(Predicate<DimensionValue> match) => Dimensions.Find(match);

        /// <summary>
        /// Returns an enumerator that iterates through the dimension values in order.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the dimension values.</returns>
        public virtual IEnumerator<DimensionValue> GetEnumerator() => Dimensions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Dimensions.GetEnumerator();

        IEnumerator<IReadOnlyDimensionValue> IEnumerable<IReadOnlyDimensionValue>.GetEnumerator()
            => Dimensions.GetEnumerator();
    }

    /// <summary>
    /// Ordered collection of content dimension values with utility methods.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ContentValueList"/> class.
    /// </remarks>
    /// <param name="dimensions">The collection of content dimension values in order.</param>
    public class ContentValueList(IEnumerable<ContentDimensionValue> dimensions) : ValueList(dimensions)
    {

        /// <summary>
        /// Gets the <see cref="ContentDimensionValue"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The <see cref="ContentDimensionValue"/> at the specified index.</returns>
        public override ContentDimensionValue this[int index]
            => (ContentDimensionValue)Dimensions[index];

        /// <summary>
        /// Finds the <see cref="ContentDimensionValue"/> with the specified code.
        /// </summary>
        /// <param name="code">The code of the value to find.</param>
        /// <returns>The <see cref="ContentDimensionValue"/> with the specified code, or null if not found.</returns>
        public override ContentDimensionValue? Find(string code)
            => (ContentDimensionValue?)Dimensions.Find(d => d.Code == code);

        /// <summary>
        /// Returns the first <see cref="ContentDimensionValue"/> that fills the specified condition.
        /// </summary>
        /// <param name="match">The condition the check the value against.</param>
        /// <returns> The first <see cref="ContentDimensionValue"/> that matches the specified predicate, or null if not found.</returns>
        public ContentDimensionValue? Find(Predicate<ContentDimensionValue> match)
        {
            foreach (var dimension in Dimensions)
            {
                if (dimension is ContentDimensionValue value && match(value))
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the content dimension values.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the content dimension values.</returns>
        public override IEnumerator<ContentDimensionValue> GetEnumerator()
            => Dimensions.Cast<ContentDimensionValue>().GetEnumerator();
    }
}
