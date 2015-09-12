using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class Transaction : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement => $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(AccountId)} INTEGER,
                        {nameof(PayeeId)} INTEGER,
                        {nameof(CategoryId)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Outflow)} FLOAT,
                        {nameof(Inflow)} FLOAT,
                        {nameof(Cleared)} INTEGER);";

        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Account Account { get; set; }

        public long AccountId
        {
            get { return Account?.Id ?? -1; }
            set { Account.Id = value; }
        }

        public DateTime Date { get; set; }

        [Write(false)]
        public Payee Payee { get; set; }

        public long PayeeId
        {
            get { return Payee?.Id ?? -1; }
            set { Payee.Id = value; }
        }

        [Write(false)]
        public Category Category { get; set; }

        public long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category.Id = value; }
        }

        public string Memo { get; set; }
        
        public double Outflow { get; set; }
        
        public double Inflow { get; set; }
        
        public bool Cleared { get; set; }

        #endregion

        #region Methods

        #endregion

        #endregion

        #region Static

        #region Static Variables



        #endregion

        #region Static Methods

        public static implicit operator Transaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
            Transaction ret = new Transaction
            {
                Account = Account.GetOrCreate(ynabTransaction.Account),
                Date = ynabTransaction.Date,
                Payee = Payee.GetOrCreate(ynabTransaction.Payee),
                Category = tempCategory,
                Memo = ynabTransaction.Memo,
                Outflow = ynabTransaction.Outflow,
                Inflow = ynabTransaction.Inflow,
                Cleared = ynabTransaction.Cleared
            };
            return ret;
        }

        #endregion

        #endregion
    }
}
