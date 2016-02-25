using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.Helper;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class Account : CommonProperty
    {
        public static AllAccounts allAccounts;

        private long _startingBalance;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public virtual long StartingBalance
        {
            get { return _startingBalance; }
            set
            {
                _startingBalance = value;
                Update();
                OnPropertyChanged();
                allAccounts?.RefreshStartingBalance();
            }
        }

        /// <summary>
        /// Representing String
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="name">Name of the Account</param>
        /// <param name="startingBalance">Starting balance of the Account</param>
        public Account(long id = -1L, string name = null, long startingBalance = 0L) : base(name)
        {
            ConstrDbLock = true;

            if (id > 0L) Id = id;
            if (_startingBalance == 0L) _startingBalance = startingBalance;

            ConstrDbLock = false;
        }

        private static readonly Dictionary<string, Account> Cache = new Dictionary<string, Account>();

        // todo: Refactor the GetOrCreate and GetAllCache into the Conversion/Import class
        public static Account GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            Account account = new Account {Name = name};
            Cache.Add(name, account);
            return account;
        }

        public static List<Account> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }

        protected override void InsertToDb()
        {
            Database?.Insert(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
        }

        protected override void DeleteFromDb()
        {
            Database?.Delete(this);
        }

        public virtual void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
            allAccounts?.RefreshBalance();
        }

        #region ViewModel_Part

        private VirtualizingObservableCollection<TitBase> _tits; 

        [Write(false)]
        public VirtualizingObservableCollection<TitBase> Tits => _tits ?? (_tits = new VirtualizingObservableCollection<TitBase>(new PaginationManager<TitBase>(new PagedTitBaseProviderAsync(Database, this))));

        [Write(false)]
        public ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();

        [Write(false)]
        public virtual long? Balance
        {
            get
            {
                return Database?.GetAccountBalance(this);
            }
            set { OnPropertyChanged(); }
        }

        [Write(false)]
        public ObservableCollection<Account> AllAccounts => Database?.AllAccounts;

        [Write(false)]
        public ObservableCollection<Payee> AllPayees => Database?.AllPayees;

        [Write(false)]
        public ObservableCollection<Category> AllCategories => Database?.AllCategories;

        [Write(false)]
        public virtual ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transaction(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false, Account = this });
        });

        [Write(false)]
        public virtual ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Income(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false, Account = this });
        });

        [Write(false)]
        public ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transfer(DateTime.Today) { Memo = "", Sum = 0L, Cleared = false });
        });

        [Write(false)]
        public virtual ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransaction(DateTime.Today) { Memo = "", Cleared = false, Account = this });
        });

        [Write(false)]
        public virtual ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncome(DateTime.Today) { Memo = "", Cleared = false, Account = this });
        });

        [Write(false)]
        public virtual ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
        }, obj => NewTits.Count > 0);

        protected void ApplyTits()
        {
            List<Account> accounts = new List<Account>();
            List<TitBase> insertTits = NewTits.Where(tit => tit.ValidToInsert()).ToList();
            foreach (TitBase tit in insertTits)
            {
                tit.Insert();
                NewTits.Remove(tit);
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
                if(tit is TitNoTransfer && !accounts.Contains((tit as TitNoTransfer).Account))
                    accounts.Add((tit as TitNoTransfer).Account);
                if (tit is Transfer)
                {
                    Transfer transfer = tit as Transfer;
                    if(!accounts.Contains(transfer.FromAccount)) accounts.Add(transfer.FromAccount);
                    if(!accounts.Contains(transfer.ToAccount)) accounts.Add(transfer.ToAccount);
                }
            }
            OnPropertyChanged(nameof(Tits));
            foreach(Account account in accounts)
                account.RefreshBalance();
        }

        [Write(false)]
        public bool IsDateFormatLong => BffEnvironment.CultureProvider?.DateLong ?? false;

        #endregion
    }
}
