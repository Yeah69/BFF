using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class Transfer : DataModelBase
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
            set { FromAccount.Id = value; }
        }

        [Write(false)]
        public Account ToAccount { get; set; }

        public long ToAccountId
        {
            get { return ToAccount?.Id ?? -1; }
            set { ToAccount.Id = value; }
        }

        public DateTime Date { get; set; }

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

        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(FromAccountId)} INTEGER,
                        {nameof(ToAccountId)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Outflow)} FLOAT,
                        {nameof(Inflow)} FLOAT,
                        {nameof(Cleared)} INTEGER);";

        #endregion

        #region Static Methods

        public static implicit operator Transfer(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
            Transfer ret = new Transfer
            {
                //todo: From and To
                Date = ynabTransaction.Date,
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
