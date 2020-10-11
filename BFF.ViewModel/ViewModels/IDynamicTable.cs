using System.Collections.Generic;
using BFF.ViewModel.Helper;

namespace BFF.ViewModel.ViewModels
{
    public interface ITableRowViewModel<TRowHeader, TCell>
    {
        TRowHeader RowHeader { get; }

        IList<TCell> Cells { get; }

        void Reset();
    }

    public interface ITableViewModel<TColumnHeader, TRowHeader, TCell>
    {
        IList<TColumnHeader> ColumnHeaders { get; }

        IReadOnlyList<ITableRowViewModel<TRowHeader, TCell>> Rows { get; }

        int ColumnCount { get; set; }
        
        int MinColumnCount { get; }
        
        int MaxColumnCount { get; }

        void Reset();
    }
}
