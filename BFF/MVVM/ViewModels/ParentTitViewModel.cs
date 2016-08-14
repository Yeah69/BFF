using System.Collections.Generic;
using BFF.MVVM.Models.Native;

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

        private ITransInc _parentTit;

        public ITransInc ParentTit
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

        public IList<ITransInc> ParentTitSource => new List<ITransInc> {ParentTit};

        public ParentTitViewModel(ITransInc parent, string title, Account account)
        {
            _title = title;
            _account = account;
            _parentTit = parent;
        }
    }
}
