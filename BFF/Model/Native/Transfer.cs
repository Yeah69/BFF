﻿using System;
using BFF.Helper.Conversion;
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

        //todo: Make FromAccountId and ToAccountId foreign keys
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Transfer)}s](
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
            //todo: Match Memo with regex
            Transfer ret = new Transfer
            {
                FromAccount = Account.GetOrCreate(ynabTransaction.Account),
                ToAccount = Account.GetOrCreate(YnabConversion.PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value),
                Date = ynabTransaction.Date,
                Memo = YnabConversion.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
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
