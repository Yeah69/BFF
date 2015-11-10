using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class Transfer : TitBase
    {
        [Key]
        public override long Id { get; set; } = -1;

        public long FillerId { get; set; } = -1;

        [Write(false)]
        public Account FromAccount { get; set; }

        public long FromAccountId
        {
            get { return FromAccount?.Id ?? -1; }
            set { FromAccount = GetAccount(value); }
        }

        [Write(false)]
        public Account ToAccount { get; set; }

        public long ToAccountId
        {
            get { return ToAccount?.Id ?? -1; }
            set { ToAccount = GetAccount(value); }
        }

        public override DateTime Date { get; set; }

        public override string Memo { get; set; }
        
        public override long? Sum { get; set; }
        
        public override bool Cleared { get; set; }

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
