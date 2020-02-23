using System.Collections.Generic;

namespace BFF.ViewModel.ViewModels
{
    public interface ITableRowViewModel<out TRowHeader, out TCell>
    {
        TRowHeader RowHeader { get; }

        TCell this[int index] { get; }
    }

    public interface ITableViewModel<out TColumnHeader, out TRowHeader, out TCell>
    {
        IReadOnlyList<TColumnHeader> ColumnHeaders { get; }

        IReadOnlyList<ITableRowViewModel<TRowHeader, TCell>> Rows { get; }

        int ColumnCount { get; }
    }
}
