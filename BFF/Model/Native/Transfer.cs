using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class Transfer : DataModelBase, ITransactionLike
    {
        #region Non-Static

        #region Properties

        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Account FromAccount { get; set; }

        public long FromAccountId
        {
            get { return FromAccount?.Id ?? -1; }
            set { FromAccount = Account.GetFromDb(value); }
        }

        [Write(false)]
        public Account ToAccount { get; set; }

        public long ToAccountId
        {
            get { return ToAccount?.Id ?? -1; }
            set { ToAccount = Account.GetFromDb(value); }
        }

        public DateTime Date { get; set; }

        public string Memo { get; set; }
        
        public double Sum { get; set; }
        
        public bool Cleared { get; set; }

        #endregion

        #region Methods

        #endregion

        #endregion

        #region Static

        #region Static Variables
        
        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(FromAccountId)} INTEGER,
                        {nameof(ToAccountId)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Sum)} FLOAT,
                        {nameof(Cleared)} INTEGER,
                        FOREIGN KEY({nameof(FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT,
                        FOREIGN KEY({nameof(ToAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT);";

        #endregion

        #region Static Methods

        public static implicit operator Transfer(YNAB.Transaction ynabTransaction)
        {
            double tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
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

        #endregion

        #endregion
    }
}
