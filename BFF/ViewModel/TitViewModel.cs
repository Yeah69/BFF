﻿using System.Collections.ObjectModel;
using BFF.DB.SQLite;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ObservableObject
    {
        public ObservableCollection<TitBase> Tits { get; set; }

        public long AccountBalance => SqLiteHelper.GetAccountBalance(_account);

        public ObservableCollection<Account> AllAccounts
        {
            get { return _allAccounts; }
            set
            {
                _allAccounts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Payee> AllPayees
        {
            get { return _allPayees; }
            set
            {
                _allPayees = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> AllCategoryRoots
        {
            get { return _allCategoryRoots; }
            set
            {
                _allCategoryRoots = value;
                OnPropertyChanged();
            }
        }

        private readonly Account _account;
        private ObservableCollection<Account> _allAccounts;
        private ObservableCollection<Payee> _allPayees;
        private ObservableCollection<Category> _allCategoryRoots;

        public TitViewModel(ObservableCollection<Account> allAccounts, ObservableCollection<Payee> allPayees, ObservableCollection<Category> allCategoryRoots, Account account = null)
        {
            AllAccounts = allAccounts;
            AllPayees = allPayees;
            AllCategoryRoots = allCategoryRoots;
            _account = account;
            Tits = new ObservableCollection<TitBase>((account == null) ? SqLiteHelper.GetAllTransactions(): SqLiteHelper.GetAllTransactions(account));
        }
    }
}
