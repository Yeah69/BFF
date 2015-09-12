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
                        {nameof(ID)} INTEGER PRIMARY KEY,
                        {nameof(AccountID)} INTEGER,
                        {nameof(PayeeID)} INTEGER,
                        {nameof(CategoryID)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Outflow)} FLOAT,
                        {nameof(Inflow)} FLOAT,
                        {nameof(Cleared)} INTEGER);";

        [Key]
        public override long ID { get; set; } = -1;

        [Write(false)]
        public Account Account { get; set; }

        public long AccountID
        {
            get { return Account?.ID ?? -1; }
            set { Account.ID = value; }
        }

        public DateTime Date { get; set; }

        [Write(false)]
        public Payee Payee { get; set; }

        public long PayeeID
        {
            get { return Payee?.ID ?? -1; }
            set { Payee.ID = value; }
        }

        [Write(false)]
        public Category Category { get; set; }

        public long CategoryID
        {
            get { return Category?.ID ?? -1; }
            set { Category.ID = value; }
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
            Transaction ret = new Transaction();
            ret.Account = Native.Account.GetOrCreate(ynabTransaction.Account);
            ret.Date = ynabTransaction.Date;
            ret.Payee = Native.Payee.GetOrCreate(ynabTransaction.Payee);
            //todo: getOrCreate Category
            if (ynabTransaction.SubCategory == string.Empty)
                ret.Category = Native.Category.GetOrCreate(ynabTransaction.MasterCategory);
            else
                ret.Category = Native.Category.GetOrCreate(
                    $"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
            ret.Memo = ynabTransaction.Memo;
            ret.Outflow = ynabTransaction.Outflow;
            ret.Inflow = ynabTransaction.Inflow;
            ret.Cleared = ynabTransaction.Cleared;
            return ret;
        }

        #endregion

        #endregion
    }
}
