using System.Collections.ObjectModel;

namespace BFF.Model.Native.Structure
{
    interface IParentTitNoTransfer<T> where T : SubTitBase
    {
        ObservableCollection<T> SubElements { get; }
        ObservableCollection<T> NewSubElements { get; }
    }
}
