using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BFF.Model.Native.Structure
{
    interface IParentTitNoTransfer<T> where T : SubTitBase
    {
        ObservableCollection<T> SubElements { get; }
        ObservableCollection<T> NewSubElements { get; }
        ICommand OpenParentTitView { get; }
    }
}
