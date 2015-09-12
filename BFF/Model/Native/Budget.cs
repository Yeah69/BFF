using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    [Table(nameof(Budget))]
    class Budget : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement { get; }

        [Key]
        public override int ID { get; set; }
        
        public DateTime MonthYear { get; set; }
        

        //Todo: budget relevant properties

        #endregion

        #region Methods

        

        #endregion

        #endregion

        #region Static

        #region Static Variables



        #endregion

        #region Static Methods



        #endregion

        #endregion
    }
}
