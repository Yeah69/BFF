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
        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Account Account { get; set; }

        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = GetAccount(value); }
        }

        public override DateTime Date { get; set; }

        [Write(false)]
        public Payee Payee { get; set; }

        public long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = GetPayee(value); }
        }

        [Write(false)]
        public Category Category { get; set; }

        public long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = GetCategory(value); }
        }

        public override string Memo { get; set; }
        
        public override long? Sum { get; set; }
        
        public override bool Cleared { get; set; }

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
