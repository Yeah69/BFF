using System;
using System.Collections.Generic;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class Transaction : TransactionIncome, ITransactionLike
    {
        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public override Account Account { get; set; }

        public override long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = GetAccount(value); }
        }

        public override DateTime Date { get; set; }

        [Write(false)]
        public override Payee Payee { get; set; }

        public override long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = GetPayee(value); }
        }

        [Write(false)]
        public override Category Category { get; set; }

        public override long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = GetCategory(value);}
        }

        public override string Memo { get; set; }

        //todo: Resolve the issue with the Parent-Transactions => Get Sum from Child-Transactions
        public override double? Sum { get; set; }

        public override bool Cleared { get; set; }

        [Write(false)]
        public IEnumerable<SubTransaction> SubTransactions { get; set; } = null;

        public static implicit operator Transaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
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
