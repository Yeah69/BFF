using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class Income : TransactionIncome, ITransactionLike
    {
        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public override Account Account { get; set; }

        public override long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account = Account.GetFromDb(value); }
        }

        public override DateTime Date { get; set; }

        [Write(false)]
        public override Payee Payee { get; set; }

        public override long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee = Payee.GetFromDb(value); }
        }

        [Write(false)]
        public override Category Category { get; set; }

        public override long? CategoryId
        {
            get { return Category?.Id; }
            set { Category = Category.GetFromDb(value); }
        }

        public override string Memo { get; set; }
        
        public override double? Sum { get; set; }
        
        public override bool Cleared { get; set; }
        
        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Income)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(AccountId)} INTEGER,
                        {nameof(PayeeId)} INTEGER,
                        {nameof(CategoryId)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Sum)} FLOAT,
                        {nameof(Cleared)} INTEGER,
                        FOREIGN KEY({nameof(AccountId)}) REFERENCES {nameof(Native.Account)}s({nameof(Native.Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(PayeeId)}) REFERENCES {nameof(Native.Payee)}s({nameof(Native.Payee.Id)}) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(CategoryId)}) REFERENCES {nameof(Native.Category)}s({nameof(Native.Category.Id)}) ON DELETE SET NULL);";

        public static implicit operator Income(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
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
