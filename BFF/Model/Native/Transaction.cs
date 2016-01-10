using System;
using System.Collections.Generic;
using System.Linq;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    public class Transaction : TitBase
    {
        private bool _cleared;
        private long? _sum;
        private string _memo;
        private Category _category;
        private Payee _payee;
        private DateTime _date;
        private Account _account;

        [Write(false)]
        public Account Account
        {
            get { return _account; }
            set
            {
                _account = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = Database?.GetAccount(value); }
        }

        public override DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        [Write(false)]
        public Payee Payee
        {
            get { return _payee; }
            set
            {
                _payee = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        public long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = Database?.GetPayee(value); }
        }

        [Write(false)]
        public Category Category
        {
            get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        public long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = Database?.GetCategory(value ?? -1L);}
        }

        public override string Memo
        {
            get { return _memo; }
            set
            {
                _memo = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        //todo: Resolve the issue with the Parent-Transactions => Get Sum from Child-Transactions
        public override long? Sum
        {
            get
            {
                return _sum ?? SubElements.Sum(subTransaction => subTransaction.Sum);
            }
            set
            {
                if (Type == "SingleTrans")
                {
                    _sum = value;
                    Database?.Update(this);
                }
                OnPropertyChanged();
            }
        }

        public override bool Cleared
        {
            get { return _cleared; }
            set
            {
                _cleared = value;
                OnPropertyChanged();
                Database?.Update(this);
            }
        }

        [Write(false)]
        public IEnumerable<SubTransaction> SubElements {
            get { return Type == "SingleTrans" ? new List<SubTransaction>(0) : Database?.GetSubTransInc<SubTransaction>(Id);}
            set { }
        }

        public override string Type { get; set; } = "SingleTrans";

        public static implicit operator Transaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            Transaction ret = new Transaction
            {
                Account = Account.GetOrCreate(ynabTransaction.Account),
                Date = ynabTransaction.Date,
                Payee = Payee.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value),
                Category = tempCategory,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }
    }
}
