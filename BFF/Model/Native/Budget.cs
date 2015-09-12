using System;
using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Budget : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement { get; }

        [Key]
        public override long ID { get; set; } = -1;

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
