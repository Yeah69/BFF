using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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
            get
            {
                return ParentTransInc.AccountId == -1 ? null : 
                    Orm?.CommonPropertyProvider?.GetAccount(ParentTransInc.AccountId);
            }
            set
            {
                Account temp = Account;
                ParentTransInc.AccountId = value?.Id ?? -1;
                Update();
                if (temp != null) Messenger.Default.Send(AccountMessage.Refresh, temp);
                if (value != null) Messenger.Default.Send(AccountMessage.Refresh, value);
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
            get
            {
                return ParentTransInc.PayeeId == -1 ? null :
                  Orm?.CommonPropertyProvider?.GetPayee(ParentTransInc.PayeeId);
            }
            set
            {
                ParentTransInc.PayeeId = value?.Id ?? -1;
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
            set { RefreshSum(); }
        }

        internal void RefreshSum()
        {
            Messenger.Default.Send(AllAccountMessage.RefreshBalance);
            Messenger.Default.Send(AccountMessage.RefreshBalance, Account);
            OnPropertyChanged(nameof(Sum));
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

        private ObservableCollection<SubTransIncViewModel<T>> _subElements; 

        public ObservableCollection<SubTransIncViewModel<T>> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    IEnumerable<T> subs = Orm?.GetSubTransInc<T>(ParentTransInc.Id) ?? new List<T>();
                    _subElements = new ObservableCollection<SubTransIncViewModel<T>>();
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

        protected abstract SubTransIncViewModel<T> CreateNewSubViewModel(T subElement);

        private readonly ObservableCollection<SubTransIncViewModel<T>> _newSubElements = new ObservableCollection<SubTransIncViewModel<T>>();
        private long _sum;

        public ObservableCollection<SubTransIncViewModel<T>> NewSubElements
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

        public void RemoveSubElement(SubTransIncViewModel<T> toRemove)
        {
            if (SubElements.Contains(toRemove))
                SubElements.Remove(toRemove);
            RefreshSum();
        }

        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            foreach (SubTransIncViewModel<T> subTransaction in SubElements)
                subTransaction.Delete();
            SubElements.Clear();
            NewSubElements.Clear();
            Delete();
        });
        
        public ICommand NewSubElementCommand => new RelayCommand(obj => _newSubElements.Add(CreateNewSubViewModel(CreateNewSubElement())));

        public abstract T CreateNewSubElement();
        
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (SubTransIncViewModel<T> subTransaction in _newSubElements)
            {
                if (Id > 0L)
                    subTransaction.Insert();
                _subElements.Add(subTransaction);
            }
            _newSubElements.Clear();
            OnPropertyChanged(nameof(Sum));
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });

        public ICommand OpenParentTitView => new RelayCommand(param =>
                    Messenger.Default.Send(new ParentTitViewModel(ParentTransInc, "Yeah69", param as Account)));
    }
}
