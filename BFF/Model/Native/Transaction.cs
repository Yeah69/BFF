﻿using System;
using System.Collections.Generic;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    [Table("BFF_Tansaction")]
    class Transaction : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement => $@"CREATE TABLE [BFF_Tansaction](
                        {nameof(ID)} INTEGER PRIMARY KEY,
                        {nameof(AccountID)} INTEGER,
                        {nameof(Date)} DATE,
                        {nameof(Memo)} TEXT,
                        {nameof(Outflow)} FLOAT,
                        {nameof(Inflow)} FLOAT,
                        {nameof(Cleared)} INTEGER);";

        [Key]
        public override int ID { get; set; }

        [Write(false)]
        public Account Account { get; set; }

        public int AccountID
        {
            get { return Account?.ID ?? -1; }
            set { Account.ID = value; }
        }

        public DateTime Date { get; set; }

        [Write(false)]
        public Payee Payee { get; set; }

        [Write(false)]
        public Category Category { get; set; }
        
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
            //todo: Find out about converting the ID
            ret.ID = 69;
            ret.Account = Native.Account.GetOrCreate(ynabTransaction.Account);
            ret.Date = ynabTransaction.Date;
            ret.Payee = Native.Payee.GetOrCreate(ynabTransaction.Payee);
            //todo: getOrCreate Category
            if (ynabTransaction.SubCategory == string.Empty)
                ret.Category = Native.Category.GetOrCreate(ynabTransaction.MasterCategory);
            else
                ret.Category = Native.Category.GetOrCreate(string.Format("{0};{1}", ynabTransaction.MasterCategory, ynabTransaction.SubCategory));
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
