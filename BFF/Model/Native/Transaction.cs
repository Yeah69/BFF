using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Model.Native.Structure;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class Transaction : DataModelBase
    {
        #region Non-Static

        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        //todo: [DataField]
        public Account Account { get; set; }

        [DataField]
        public DateTime Date { get; set; }

        //todo: [DataField]
        public Payee Payee { get; set; }

        //todo: [DataField]
        public Category Category { get; set; }

        [DataField]
        public string Memo { get; set; }

        [DataField]
        public double Outflow { get; set; }

        [DataField]
        public double Inflow { get; set; }

        [DataField]
        public bool Cleared { get; set; }

        #endregion

        #region Methods

        protected override string GetDelimitedCreateTableList(string delimiter)
        {
            List<string> list = new List<string>{"ID INTEGER PRIMARY KEY AUTOINCREMENT",
                "Date DATE",
                "Memo TEXT",
                "Outflow FLOAT",
                "Inflow FLOAT",
                "Cleared INTEGER"
            };
            return string.Join(delimiter, list);
        }

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
