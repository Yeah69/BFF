using System.Collections.ObjectModel;
using BFF.DB;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.ViewModel.ForModels
{
    class ParentTransIncViewModel<T> : TransIncViewModel where T : ISubTransInc
    {
        public ObservableCollection<T> SubElements
        {
            get { return (TransInc as IParentTransInc<T>)?.SubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        public ObservableCollection<T> NewSubElements
        {
            get { return (TransInc as IParentTransInc<T>)?.SubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        public ParentTransIncViewModel(IParentTransInc<T> transInc, IBffOrm orm) : base(transInc, orm) {}
    }
}
