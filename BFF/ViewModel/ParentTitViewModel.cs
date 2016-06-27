using System.Collections.Generic;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
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

        public IList<TitNoTransfer> ParentTitSource => new List<TitNoTransfer> {ParentTit};

        public ParentTitViewModel(TitNoTransfer parent, string title)
        {
            _title = title;
            _parentTit = parent;
        }
    }
}
