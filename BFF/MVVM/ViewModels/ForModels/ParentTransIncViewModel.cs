using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentTransIncViewModel<T> : TransIncViewModel where T : ISubTransInc
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
                        _subElements.Add(new SubTransIncViewModel(sub, Orm));
                    }
                }
                return _subElements;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SubTransIncViewModel> _newSubElements = new ObservableCollection<SubTransIncViewModel>(); 

        public ObservableCollection<SubTransIncViewModel> NewSubElements
        {
            get { return _newSubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        public ParentTransIncViewModel(IParentTransInc<T> transInc, IBffOrm orm) : base(transInc, orm) { }

        public override bool ValidToInsert()
        {
            return Account != null && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee   != null &&  Orm .AllPayees.Contains(Payee) && NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        public override void Insert()
        {
            if (TransInc is ParentTransaction)
                Orm?.Insert(TransInc as ParentTransaction);
            else if (TransInc is ParentIncome)
                Orm?.Insert(TransInc as ParentIncome);
            else
                throw new NotImplementedException($"{TransInc.GetType().FullName} is not supported as {nameof(ParentTransaction)} or {nameof(ParentIncome)}."); //todo Localization

        }
    }
}
