namespace Px.Utils.Models.Data.Collections
{
    internal interface IDataCollection<out T> : IReadOnlyList<T>
    {
        T[] ToArray();
    }
}
