using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BFF.Helper.Extensions
{
    public static class DataGridRowExtensions
    {
        public static DataGridCell GetCell(this DataGridRow rowContainer, int column)
        {
            return rowContainer?
                .FindVisualChild<DataGridCellsPresenter>()?
                .ItemContainerGenerator
                .ContainerFromIndex(column) as DataGridCell;
        }
    }
}
