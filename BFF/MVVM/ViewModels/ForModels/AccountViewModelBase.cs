using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.Properties;

namespace BFF.MVVM.ViewModels.ForModels
{
    public abstract class AccountViewModelBase : ObservableObject, IVirtualizedRefresh
    {
        protected IBffOrm Orm;
        protected VirtualizingObservableCollection<TitBase> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract long StartingBalance { get; set; }

        public abstract string Name { get; set; }
        public abstract VirtualizingObservableCollection<TitBase> Tits { get; }
        public abstract ObservableCollection<TitBase> NewTits { get; set; }
        public abstract long? Balance { get; set; }
        public ObservableCollection<Account> AllAccounts => Orm?.CommonPropertyProvider.Accounts;
        public ObservableCollection<Payee> AllPayees => Orm?.AllPayees;
        public ObservableCollection<Category> AllCategories => Orm?.AllCategories;
        public abstract ICommand NewTransactionCommand { get; }
        public abstract ICommand NewIncomeCommand { get; }
        public abstract ICommand NewTransferCommand { get; }
        public abstract ICommand NewParentTransactionCommand { get; }
        public abstract ICommand NewParentIncomeCommand { get; }
        public abstract ICommand ApplyCommand { get; }
        public bool IsDateFormatLong => Settings.Default.Culture_DefaultDateLong;
        public Account Account { get; protected set; }

        protected AccountViewModelBase(IBffOrm orm)
        {
            Orm = orm;
            Messenger.Default.Register<CutlureMessage>(this, message =>
            {
                switch (message)
                {
                    case CutlureMessage.Refresh:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(Balance));
                        RefreshTits();
                        break;
                    case CutlureMessage.RefreshCurrency:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(Balance));
                        RefreshTits();
                        break;
                    case CutlureMessage.RefreshDate:
                        RefreshTits();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            });
        }

        /// <summary>
        /// Representing String
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }

        public abstract void RefreshBalance();

        public abstract void RefreshTits();

        protected void ApplyTits()
        {
            List<AccountViewModel> accountViewModels = new List<AccountViewModel>();
            List<TitBase> insertTits = NewTits.Where(tit => tit.ValidToInsert()).ToList();
            foreach (TitBase tit in insertTits)
            {
                tit.Insert();
                NewTits.Remove(tit);
                if (tit is IParentTransInc<SubTransaction>)
                {
                    IParentTransInc<SubTransaction> parentTransaction = tit as IParentTransInc<SubTransaction>;
                    foreach (SubTransaction subTransaction in parentTransaction.NewSubElements)
                    {
                        subTransaction.Insert();
                        parentTransaction.SubElements.Add(subTransaction);
                    }
                    parentTransaction.NewSubElements.Clear();
                }
                if (tit is IParentTransInc<SubIncome>)
                {
                    IParentTransInc<SubIncome> parentIncome = tit as IParentTransInc<SubIncome>;
                    foreach (SubIncome subIncome in parentIncome.NewSubElements)
                    {
                        subIncome.Insert();
                        parentIncome.SubElements.Add(subIncome);
                    }
                    parentIncome.NewSubElements.Clear();
                }
                if (tit is TitNoTransfer)
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel((tit as TitNoTransfer).Account.Id));
                if (tit is Transfer)
                {
                    Transfer transfer = tit as Transfer;
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel(transfer.FromAccount.Id));
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel(transfer.ToAccount.Id));
                }
            }
            Orm.CommonPropertyProvider.AllAccountsViewModel.RefreshTits();
            foreach (AccountViewModel accountViewModel in accountViewModels)
            {
                accountViewModel.RefreshTits();
                accountViewModel.RefreshBalance();
            }
        }

        public virtual event EventHandler PreVirtualizedRefresh;

        public void OnPreVirtualizedRefresh()
        {
            PreVirtualizedRefresh?.Invoke(this, new EventArgs());
        }

        public virtual event EventHandler PostVirtualizedRefresh;

        public void OnPostVirtualizedRefresh()
        {
            PostVirtualizedRefresh?.Invoke(this, new EventArgs());
        }
    }
}