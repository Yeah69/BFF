using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    abstract class ParentTransIncViewModel<T> : TransIncViewModel where T : ISubTransInc
    {
        private ObservableCollection<SubTransIncViewModel> _subElements; 

        public ObservableCollection<SubTransIncViewModel> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    IEnumerable<T> subs = Orm?.GetSubTransInc<T>(TransInc.Id);
                    _subElements = new ObservableCollection<SubTransIncViewModel>();
                    foreach(T sub in subs)
                    {
                        _subElements.Add(CreateNewSubViewModel(sub)); //new SubTransIncViewModel(sub, Orm));
                    }
                }
                return _subElements;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        protected abstract SubTransIncViewModel CreateNewSubViewModel(T subElement);

        private ObservableCollection<SubTransIncViewModel> _newSubElements = new ObservableCollection<SubTransIncViewModel>(); 

        public ObservableCollection<SubTransIncViewModel> NewSubElements
        {
            get { return _newSubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        protected ParentTransIncViewModel(IParentTransInc<T> transInc, IBffOrm orm) : base(transInc, orm) { }

        internal override bool ValidToInsert()
        {
            return Account != null && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee   != null &&  Orm .AllPayees.Contains(Payee) && NewSubElements.All(subElement => subElement.ValidToInsert());
        }
    }
}
