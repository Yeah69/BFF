using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels
{
    public class ParentTitViewModel : ObservableObject
    {
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private TitNoTransfer _parentTit;

        public TitNoTransfer ParentTit
        {
            get { return _parentTit; }
            set
            {
                _parentTit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ParentTitSource));
            }
        }

        private Account _account;

        public Account Account
        {
            get { return _account; }
            set
            {
                _account = value;
                OnPropertyChanged();
            }
        }

        public IList<TitNoTransfer> ParentTitSource => new List<TitNoTransfer> {ParentTit};

        public ParentTitViewModel(TitNoTransfer parent, string title, Account account)
        {
            _title = title;
            _account = account;
            _parentTit = parent;
        }
    }
}
