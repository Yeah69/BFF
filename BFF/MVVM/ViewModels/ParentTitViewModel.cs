using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels
{
    public class ParentTitViewModel : ObservableObject
    {
        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private ITitLikeViewModel _parentTit;

        public ITitLikeViewModel ParentTit
        {
            get => _parentTit;
            set
            {
                _parentTit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ParentTitSource));
            }
        }

        private IAccount _account;

        public IAccount Account
        {
            get => _account;
            set
            {
                _account = value;
                OnPropertyChanged();
            }
        }

        public IList<ITitLikeViewModel> ParentTitSource => new List<ITitLikeViewModel> {ParentTit};

        public ParentTitViewModel(ITitLikeViewModel parent, string title, IAccount account)
        {
             _title = title;
            _account = account;
            _parentTit = parent;
        }
    }
}
