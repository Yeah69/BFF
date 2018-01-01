using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
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

        private ITransLikeViewModel _parentTransaction;

        public ITransLikeViewModel ParentTransaction
        {
            get => _parentTransaction;
            set
            {
                _parentTransaction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ParentTitSource));
            }
        }

        private IAccountViewModel _account;

        public IAccountViewModel Account
        {
            get => _account;
            set
            {
                _account = value;
                OnPropertyChanged();
            }
        }

        public IList<ITransLikeViewModel> ParentTitSource => new List<ITransLikeViewModel> {ParentTransaction};

        public ParentTitViewModel(ITransLikeViewModel parent, string title, IAccountViewModel account)
        {
             _title = title;
            _account = account;
            _parentTransaction = parent;
        }
    }
}
