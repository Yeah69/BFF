﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BFF.DB;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ViewModelBase
    {
        public ObservableCollection<TitBase> Tits { get; set; } = new ObservableCollection<TitBase>();
        public ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();

        public string Header => _account?.Name ?? "All Accounts"; //todo: Localize "All Accounts"

        public long AccountBalance => _orm.GetAccountBalance(_account);

        public ObservableCollection<Account> AllAccounts => _orm.AllAccounts;

        public ObservableCollection<Payee> AllPayees => _orm.AllPayees;

        public ObservableCollection<Category> AllCategories => _orm.AllCategories;

        public Account Account => _account;

        public ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transaction (DateTime.Today) { Memo = "", Sum=0L, Cleared = false, Account = _account});
        });

        public ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Income (DateTime.Today) { Memo = "", Sum = 0L, Cleared = false, Account = _account });
        });

        public ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transfer(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false });
        });

        public ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransaction(DateTime.Today) { Memo = "", Cleared = false, Account = _account });
        });

        public ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncome(DateTime.Today) { Memo = "", Cleared = false, Account = _account });
        });

        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (TitBase tit in NewTits)
            {
                Tits.Add(tit);
                tit.Insert();
                if (tit is IParentTitNoTransfer<SubTransaction>)
                {
                    IParentTitNoTransfer<SubTransaction> parentTransaction = tit as IParentTitNoTransfer<SubTransaction>;
                    foreach (SubTransaction subTransaction in parentTransaction.NewSubElements)
                    {
                        subTransaction.Insert();
                        parentTransaction.SubElements.Add(subTransaction);
                    }
                    parentTransaction.NewSubElements.Clear();
                }
                if (tit is IParentTitNoTransfer<SubIncome>)
                {
                    IParentTitNoTransfer<SubIncome> parentIncome = tit as IParentTitNoTransfer<SubIncome>;
                    foreach (SubIncome subIncome in parentIncome.NewSubElements)
                    {
                        subIncome.Insert();
                        parentIncome.SubElements.Add(subIncome);
                    }
                    parentIncome.NewSubElements.Clear();
                }
            }
            OnPropertyChanged(nameof(Tits));//todo:Validate correctness
            NewTits.Clear(); 
        }, obj => NewTits.Count > 0);

        private readonly Account _account;
        private readonly IBffOrm _orm;


        public TitViewModel(IBffOrm orm, Account account = null)
        {
             _orm = orm;
            _account = account;
            Refresh();
        }

        public override void Refresh()
        {
            Tits.Clear();
            NewTits.Clear();
            foreach(TitBase titBase in _orm.GetAllTits(_account))
                Tits.Add(titBase);
        }
    }
}
