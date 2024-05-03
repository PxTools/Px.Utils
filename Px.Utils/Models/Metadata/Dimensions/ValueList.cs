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
        protected List<DimensionValue> Values { get; }

        /// <summary>
        /// Gets the dimension value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the dimension value to get.</param>
        /// <returns>The dimension value at the specified index.</returns>
        public virtual DimensionValue this[int index] => Values[index];

        IReadOnlyDimensionValue IReadOnlyList<IReadOnlyDimensionValue>.this[int index] => Values[index];

        /// <summary>
        /// Gets the number of dimension values in the list.
        /// </summary>
        public int Count => Values.Count;

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
            Values = dimensions.ToList();
            Codes = Values.Select(d => d.Code).ToList();
        }

        /// <summary>
        /// Finds the dimension value with the specified code.
        /// </summary>
        /// <param name="code">The code of the dimension value to find.</param>
        /// <returns>The dimension value with the specified code, or null if not found.</returns>
        public virtual DimensionValue? Find(string code) => Values.Find(d => d.Code == code);

        /// <summary>
        /// Returns the first dimension value that fills the specified condition.
        /// </summary>
        /// <param name="match">The condition the check the dimension value against.</param>
        /// <returns>The first dimension value that matches the specified predicate, or null if not found.</returns>
        public DimensionValue? Find(Predicate<DimensionValue> match) => Values.Find(match);

        /// <summary>
        /// Adds a dimension value to the end of the list.
        /// </summary>
        /// <param name="value">Value to add.</param>
        public void Add(DimensionValue value) => Values.Add(value);

        /// <summary>
        /// Inserts a dimension value into the list at the specified index.
        /// </summary>
        /// <param name="index">Index at which to insert the value.</param>
        /// <param name="value">Value to be added.</param>
        public void Insert(int index, DimensionValue value) => Values.Insert(index, value);

        /// <summary>
        /// Removes the dimension value with the specified code from the list.
        /// </summary>
        /// <param name="code">Code of the dimension value to remove.</param>
        /// <exception cref="ArgumentException">Thrown when the dimension value with the specified code is not found in the list.</exception>
        public void Remove(string code)
        {
            if (Find(code) is DimensionValue value)
            {
                Values.Remove(value);
            }
            else
            {
                throw new ArgumentException($"Dimension value with code {code} not found in value list");
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dimension values in order.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the dimension values.</returns>
        public virtual IEnumerator<DimensionValue> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();

        IEnumerator<IReadOnlyDimensionValue> IEnumerable<IReadOnlyDimensionValue>.GetEnumerator()
            => Values.GetEnumerator();
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
            => (ContentDimensionValue)Values[index];

        /// <summary>
        /// Finds the <see cref="ContentDimensionValue"/> with the specified code.
        /// </summary>
        /// <param name="code">The code of the value to find.</param>
        /// <returns>The <see cref="ContentDimensionValue"/> with the specified code, or null if not found.</returns>
        public override ContentDimensionValue? Find(string code)
            => (ContentDimensionValue?)Values.Find(d => d.Code == code);

        /// <summary>
        /// Returns the first <see cref="ContentDimensionValue"/> that fills the specified condition.
        /// </summary>
        /// <param name="match">The condition the check the value against.</param>
        /// <returns> The first <see cref="ContentDimensionValue"/> that matches the specified predicate, or null if not found.</returns>
        public ContentDimensionValue? Find(Predicate<ContentDimensionValue> match)
        {
            foreach (var dimension in Values)
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
            => Values.Cast<ContentDimensionValue>().GetEnumerator();
    }
}
