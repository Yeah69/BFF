using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public abstract class ViewModelBase : ObservableObject
    {
        public abstract void Refresh();
    }
}
