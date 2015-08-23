using System;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Transaction : DataModelBase
    {

        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public Account Account { get; set; }

        [DataField]
        public DateTime Date { get; set; }

        [DataField]
        public Payee Payee { get; set; }

        [DataField]
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
        
    }
}
