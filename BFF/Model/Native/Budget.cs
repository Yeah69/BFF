using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Budget : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Key]
        public override long Id { get; set; } = -1;

        public DateTime MonthYear { get; set; }


        //Todo: budget relevant properties

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(MonthYear)} DATE;";
        
        #endregion

        #region Static Methods



        #endregion

        #endregion
    }
}
