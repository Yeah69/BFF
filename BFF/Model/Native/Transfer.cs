using System;
using System.Windows.Input;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    /// <summary>
    /// A Transfer is basically a Transaction from one owned Account to another owned Account
    /// </summary>
    public class Transfer : TitBase
    {
        private Account _fromAccount;
        private Account _toAccount;

        /// <summary>
        /// The Sum is transfered from this Account
        /// </summary>
        [Write(false)]
        public Account FromAccount
        {
            get { return _fromAccount; }
            set
            {
                if (_toAccount == value)
                {
                    _toAccount = _fromAccount;
                    OnPropertyChanged(nameof(ToAccount));
                }
                _fromAccount = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of FromAccount
        /// </summary>
        public long FromAccountId
        {
            get { return FromAccount?.Id ?? -1; }
            set { FromAccount = Database?.GetAccount(value); }
        }


        /// <summary>
        /// The Sum is transfered to this Account
        /// </summary>
        [Write(false)]
        public Account ToAccount
        {
            get { return _toAccount; }
            set
            {
                if (_fromAccount == value)
                {
                    _fromAccount = _toAccount;
                    OnPropertyChanged(nameof(FromAccount));
                }
                _toAccount = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of ToAccount
        /// </summary>
        public long ToAccountId
        {
            get { return ToAccount?.Id ?? -1; }
            set { ToAccount = Database?.GetAccount(value); }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="fromAccount">The Sum is transfered from this Account</param>
        /// <param name="toAccount">The Sum is transfered to this Account</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The transfered Sum</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public Transfer(DateTime date, Account fromAccount = null, Account toAccount = null, string memo = null,
            long sum = 0L, bool? cleared = null)
            : base(date, memo: memo, sum: sum, cleared: cleared)
        {
            ConstrDbLock = true;

            _fromAccount = fromAccount ?? _fromAccount;
            _toAccount = toAccount ?? _toAccount;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="fromAccountId">Id of FromAccount</param>
        /// <param name="toAccountId">Id of ToAccount</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The transfered Sum</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        /// <param name="type">Indicates the Tit-Type and if it is a Single or Parent</param>
        public Transfer(long id, long fromAccountId, long toAccountId, DateTime date, string memo,
            long sum, bool cleared)
            : base(date, id, memo, sum, cleared)
        {
            ConstrDbLock = true;

            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Creates a Transfer-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        public static implicit operator Transfer(YNAB.Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            Account tempFromAccount = tempSum < 0 ? Account.GetOrCreate(ynabTransaction.Account) 
                : Account.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value);
            Account tempToAccount = tempSum >= 0 ? Account.GetOrCreate(ynabTransaction.Account) 
                : Account.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value);
            Transfer ret = new Transfer(ynabTransaction.Date)
            {
                FromAccount = tempFromAccount,
                ToAccount = tempToAccount,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = Math.Abs(tempSum),
                Cleared = ynabTransaction.Cleared
            };
            return ret;
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

        [Write(false)]
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            FromAccount.RefreshBalance();
            ToAccount.RefreshBalance();
            for (int i = FromAccount.Tits.Count - 1; i >= 0; i--)
            {
                if (FromAccount.Tits[i].GetType() == GetType() && FromAccount.Tits[i].Id == Id)
                {
                    FromAccount.Tits.Remove(FromAccount.Tits[i]);
                    break;
                }
            }
            for (int i = ToAccount.Tits.Count - 1; i >= 0; i--)
            {
                if (ToAccount.Tits[i].GetType() == GetType() && ToAccount.Tits[i].Id == Id)
                {
                    ToAccount.Tits.Remove(ToAccount.Tits[i]);
                    break;
                }
            }
            for (int i = Account.allAccounts.Tits.Count - 1; i >= 0; i--)
            {
                if (Account.allAccounts.Tits[i].GetType() == GetType() && Account.allAccounts.Tits[i].Id == Id)
                {
                    Account.allAccounts.Tits.Remove(Account.allAccounts.Tits[i]);
                    break;
                }
            }
        });
    }
}
