using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Properties;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class AccountBaseViewModel : DataModelViewModel, IVirtualizedRefresh
    {
        protected VirtualizingObservableCollection<TitLikeViewModel> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract long StartingBalance { get; set; }

        public abstract string Name { get; set; }
        public abstract VirtualizingObservableCollection<TitLikeViewModel> Tits { get; }
        public abstract ObservableCollection<TitLikeViewModel> NewTits { get; set; }
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

        protected AccountBaseViewModel(IBffOrm orm) : base(orm)
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
            List<TitLikeViewModel> insertTits = NewTits.Where(tit => tit.ValidToInsert()).ToList();
            foreach (TitLikeViewModel tit in insertTits)
            {
                tit.Insert();
                NewTits.Remove(tit);
                if (tit is ParentTransactionViewModel)
                {
                    ParentTransactionViewModel parentTransaction = tit as ParentTransactionViewModel;
                    foreach (SubTransIncViewModel<SubTransaction> subTransaction in parentTransaction.NewSubElements)
                    {
                        subTransaction.Insert();
                        parentTransaction.SubElements.Add(subTransaction);
                    }
                    parentTransaction.NewSubElements.Clear();
                }
                else if (tit is ParentIncomeViewModel) // todo unify this and the above if-clause?
                {
                    ParentIncomeViewModel parentIncome = tit as ParentIncomeViewModel;
                    foreach (SubTransIncViewModel<SubIncome> subIncome in parentIncome.NewSubElements)
                    {
                        subIncome.Insert();
                        parentIncome.SubElements.Add(subIncome);
                    }
                    parentIncome.NewSubElements.Clear();
                }
                else if (tit is TransIncViewModel)
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel((tit as TransIncViewModel).Account.Id));
                else if (tit is TransferViewModel)
                {
                    TransferViewModel transfer = tit as TransferViewModel;
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