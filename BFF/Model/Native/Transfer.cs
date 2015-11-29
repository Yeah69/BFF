﻿using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class Transfer : TitBase
    {
        private Account _fromAccount;
        private Account _toAccount;
        private DateTime _date;
        private string _memo;
        private long? _sum;
        private bool _cleared;

        [Key]
        public override long Id { get; set; } = -1;

        public long FillerId { get; set; } = -1;

        [Write(false)]
        public Account FromAccount
        {
            get { return _fromAccount; }
            set
            {
                _fromAccount = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public long FromAccountId
        {
            get { return FromAccount?.Id ?? -1; }
            set { FromAccount = GetAccount(value); }
        }

        [Write(false)]
        public Account ToAccount
        {
            get { return _toAccount; }
            set
            {
                _toAccount = value;
                OnPropertyChanged();
                Update(this);
            }
        }

        public long ToAccountId
        {
            get { return ToAccount?.Id ?? -1; }
            set { ToAccount = GetAccount(value); }
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

        public override string Type { get; set; } = "Transfer";

        public static implicit operator Transfer(YNAB.Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            Account tempFromAccount = (tempSum < 0) ? Account.GetOrCreate(ynabTransaction.Account) 
                : Account.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value);
            Account tempToAccount = (tempSum >= 0) ? Account.GetOrCreate(ynabTransaction.Account) 
                : Account.GetOrCreate(YnabCsvImport.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value);
            Transfer ret = new Transfer
            {
                FromAccount = tempFromAccount,
                ToAccount = tempToAccount,
                Date = ynabTransaction.Date,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = Math.Abs(tempSum),
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }
    }
}
