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
    public interface IAccountBaseViewModel : ICommonPropertyViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        long StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        VirtualizingObservableCollection<ITitLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        ObservableCollection<ITitLikeViewModel> NewTits { get; }

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        long? Balance { get; }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        ICommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        ICommand NewIncomeCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        ICommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        ICommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        ICommand NewParentIncomeCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        ICommand ApplyCommand { get; }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        void RefreshBalance();

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        void RefreshTits();
    }

    public abstract class AccountBaseViewModel : CommonPropertyViewModel, IVirtualizedRefresh, IAccountBaseViewModel
    {
        private readonly IAccount _account;

        protected VirtualizingObservableCollection<ITitLikeViewModel> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract long StartingBalance { get; set; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public abstract VirtualizingObservableCollection<ITitLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public abstract ObservableCollection<ITitLikeViewModel> NewTits { get; set; }

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public abstract long? Balance { get; }

        /// <summary>
        /// All available Accounts.
        /// </summary>
        public ObservableCollection<IAccount> AllAccounts => CommonPropertyProvider.Accounts;

        /// <summary>
        /// All available Payees.
        /// </summary>
        public ObservableCollection<IPayee> AllPayees => CommonPropertyProvider.Payees;

        /// <summary>
        /// All available Categories.
        /// </summary>
        public ObservableCollection<ICategory> AllCategories => CommonPropertyProvider.Categories;

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public abstract ICommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public abstract ICommand NewIncomeCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public abstract ICommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public abstract ICommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public abstract ICommand NewParentIncomeCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        public abstract ICommand ApplyCommand { get; }

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool IsDateFormatLong => Settings.Default.Culture_DefaultDateLong;

        /// <summary>
        /// The Account Model.
        /// </summary>
        public IAccount Account { get; protected set; }

        /// <summary>
        /// Initializes a AccountBaseViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="account">The model.</param>
        protected AccountBaseViewModel(IBffOrm orm, IAccount account) : base(orm, account)
        {
            _account = account;
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
                        OnPropertyChanged(nameof(IsDateFormatLong));
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            });
        }

        /// <summary>
        /// Representing String.
        /// </summary>
        /// <returns>Just the Name-property.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public abstract void RefreshBalance();

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public abstract void RefreshTits();

        /// <summary>
        /// Common logic for the Apply-Command.
        /// </summary>
        protected void ApplyTits()
        {
            List<IAccountViewModel> accountViewModels = new List<IAccountViewModel>();
            List<ITitLikeViewModel> insertTits = NewTits.Where(tit => tit.ValidToInsert()).ToList();
            foreach (ITitLikeViewModel tit in insertTits)
            {
                tit.Insert();
                NewTits.Remove(tit);
                if (tit is IParentTransactionViewModel parentTransactionViewModel)
                {
                    foreach(ISubTransIncViewModel subTransaction in parentTransactionViewModel.NewSubElements)
                    {
                        subTransaction.Insert();
                        parentTransactionViewModel.SubElements.Add(subTransaction);
                    }
                    parentTransactionViewModel.NewSubElements.Clear();
                }
                else if (tit is IParentIncomeViewModel parentIncomeViewModel) // todo unify this and the above if-clause?
                {
                    foreach (ISubTransIncViewModel subIncome in parentIncomeViewModel.NewSubElements)
                    {
                        subIncome.Insert();
                        parentIncomeViewModel.SubElements.Add(subIncome);
                    }
                    parentIncomeViewModel.NewSubElements.Clear();
                }
                else if (tit is ITransIncViewModel transIncViewModel)
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel(transIncViewModel.Account.Id));
                else if (tit is ITransferViewModel transferViewModel)
                {
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel(transferViewModel.FromAccount.Id));
                    accountViewModels.Add(Orm.CommonPropertyProvider.GetAccountViewModel(transferViewModel.ToAccount.Id));
                }
            }
            
            Refresh(Orm.CommonPropertyProvider.SummaryAccountViewModel);
            foreach (IAccountViewModel accountViewModel in accountViewModels.Distinct())
            {
                Refresh(accountViewModel);
            }
            
            void Refresh(IAccountBaseViewModel accountViewModel)
            {
                accountViewModel.RefreshTits();
                accountViewModel.RefreshBalance();
            }
        }

        /// <summary>
        /// Invoked right before the TITs are refreshed.
        /// </summary>
        public virtual event EventHandler PreVirtualizedRefresh;

        /// <summary>
        /// Invoked right before the TITs are refreshed.
        /// </summary>
        public void OnPreVirtualizedRefresh()
        {
            PreVirtualizedRefresh?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Invoked right after the TITs are refreshed.
        /// </summary>
        public virtual event EventHandler PostVirtualizedRefresh;

        /// <summary>
        /// Invoked right after the TITs are refreshed.
        /// </summary>
        public void OnPostVirtualizedRefresh()
        {
            PostVirtualizedRefresh?.Invoke(this, new EventArgs());
        }
    }
}