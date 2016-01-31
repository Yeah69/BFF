using System;
using System.Linq;
using System.Windows.Input;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AllAccounts : Account
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public override long StartingBalance
        {
            get { return Database?.AllAccounts.Sum(account => account.StartingBalance) ?? 0L; }
            set { OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public AllAccounts()
        {
            ConstrDbLock = true;

            Name = "All Accounts";
            allAccounts = this;

            ConstrDbLock = false;
        }

        protected override void InsertToDb()
        {
        }

        protected override void UpdateToDb()
        {
        }

        protected override void DeleteFromDb()
        {
        }

        public override void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
        }

        #region ViewModel_Part

        [Write(false)]
        public override long Balance
        {
            get { return AllAccounts.Sum(account => account.Balance); }
            set { OnPropertyChanged(); }
        }

        [Write(false)]
        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transaction(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false, Account = null });
        });

        [Write(false)]
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Income(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false, Account = null });
        });

        [Write(false)]
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransaction(DateTime.Today) { Memo = "", Cleared = false, Account = null });
        });

        [Write(false)]
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncome(DateTime.Today) { Memo = "", Cleared = false, Account = null });
        });

        [Write(false)]
        public override ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
            OnPropertyChanged(nameof(Balance));
        }, obj => NewTits.Count > 0);

        public override void RefreshTits()
        {
            Tits.Clear();
            foreach (TitBase titBase in IsFilterOn ? Database?.GetAllTits(FilterStartDate, FilterEndDate) : Database?.GetAllTits(DateTime.MinValue, DateTime.MaxValue))
                Tits.Add(titBase);
        }

        #endregion
    }
}
