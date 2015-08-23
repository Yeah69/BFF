using System;
using System.Collections.Generic;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Budget : DataModelBase
    {
        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public DateTime MonthYear { get; set; }

        [DataField]
        public List<Transaction> Transactions { get; set; }

        //Todo: budget relevant properties

        #endregion

    }
}
