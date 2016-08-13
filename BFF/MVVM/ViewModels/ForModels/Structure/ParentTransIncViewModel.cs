using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    abstract class ParentTransIncViewModel<T> : TransIncBaseViewModel where T : ISubTransInc
    {
        protected IParentTransInc<T> ParentTransInc;

        public override long Id => ParentTransInc.Id;

        public override Account Account
        {
            get { return ParentTransInc.Account; }
            set
            {
                ParentTransInc.Account = value;
                Update();
                OnPropertyChanged();
            }
        }

        public override DateTime Date
        {
            get { return ParentTransInc.Date; }
            set
            {
                ParentTransInc.Date = value;
                Update();
                OnPropertyChanged();
            }
        }

        public override Payee Payee
        {
            get { return ParentTransInc.Payee; }
            set
            {
                ParentTransInc.Payee = value;
                Update();
                OnPropertyChanged();
            }
        }

        public override string Memo
        {
            get { return ParentTransInc.Memo; }
            set
            {
                ParentTransInc.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        public override long Sum
        {
            get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
            set
            {
                OnPropertyChanged();
                Messenger.Default.Send(AllAccountMessage.Refresh);
                Messenger.Default.Send(AccountMessage.Refresh, Account);
            }
        }

        public override bool Cleared
        {
            get { return ParentTransInc.Cleared; }
            set
            {
                ParentTransInc.Cleared = value;
                Update();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SubTransIncViewModel> _subElements; 

        public ObservableCollection<SubTransIncViewModel> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    IEnumerable<T> subs = Orm?.GetSubTransInc<T>(ParentTransInc.Id) ?? new List<T>();
                    _subElements = new ObservableCollection<SubTransIncViewModel>();
                    foreach(T sub in subs)
                    {
                        _subElements.Add(CreateNewSubViewModel(sub));
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

        private readonly ObservableCollection<SubTransIncViewModel> _newSubElements = new ObservableCollection<SubTransIncViewModel>();
        private long _sum;

        public ObservableCollection<SubTransIncViewModel> NewSubElements
        {
            get { return _newSubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        protected ParentTransIncViewModel(IParentTransInc<T> transInc, IBffOrm orm) : base(orm)
        {
            ParentTransInc = transInc;
        }

        internal override bool ValidToInsert()
        {
            return Account != null && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee   != null &&  Orm .CommonPropertyProvider.Payees.Contains(Payee) && 
                   NewSubElements.All(subElement => subElement.ValidToInsert());
        }
    }
}
