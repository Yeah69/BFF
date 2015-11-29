using System;
using System.Collections.Generic;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class Income : TitBase
    {
        private Account _account;
        private DateTime _date;
        private Payee _payee;
        private Category _category;
        private string _memo;
        private long? _sum;
        private bool _cleared;

        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Account Account
        {
            get { return _account; }
            set
            {
                _account = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = GetAccount(value); }
        }

        public override DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
                Update(this);
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
                Update(this);
            }
        }

        public long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = GetPayee(value); }
        }

        [Write(false)]
        public Category Category
        {
            get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = GetCategory(value); }
        }

        public override string Memo
        {
            get { return _memo; }
            set
            {
                _memo = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public override long? Sum
        {
            get { return _sum; }
            set
            {
                _sum = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public override bool Cleared
        {
            get { return _cleared; }
            set
            {
                _cleared = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        [Write(false)]
        public IEnumerable<SubTransInc> SubElements
        {
            get { return GetSubIncomes(Id); }
        }

        public override string Type { get; set; } = "SingleIncome";

        public static implicit operator Income(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            Income ret = new Income
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
