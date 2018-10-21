using System.Collections.Generic;

namespace BFF.Model.Models.Structure
{
    public interface IHaveSortIndex
    {
        long SortIndex { get; set; }
    }

    internal class SortIndexComparer<T> : IComparer<T> where T : class, IHaveSortIndex
    {
        public int Compare(T x, T y)
        {
            if (x == y) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            if (x.SortIndex < y.SortIndex) return -1;
            if (x.SortIndex == y.SortIndex) return 0;
            return 1;
        }
    }
}
