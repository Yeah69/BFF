using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BFF.MVVM.Models.Native.Structure
{
    interface IParentTransInc<T> : ITransInc where T : IBasicTit 
    {
        ObservableCollection<T> SubElements { get; }
        ObservableCollection<T> NewSubElements { get; }
        ICommand OpenParentTitView { get; }
    }
}
