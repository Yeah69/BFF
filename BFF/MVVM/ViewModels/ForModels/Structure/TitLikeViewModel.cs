using System.Windows.Input;
using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TitLikeViewModel : DataModelViewModel
    {
        public abstract string Memo { get; set; }
        public abstract long Sum { get; set; }

        protected TitLikeViewModel(IBffOrm orm) : base(orm) { }

        public virtual ICommand DeleteCommand => new RelayCommand(obj => Delete());
    }
}
