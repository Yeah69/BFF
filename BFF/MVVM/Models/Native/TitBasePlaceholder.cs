using System;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    sealed class TitBasePlaceholder : TitBase
    {
        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        /// <param name="date">Marks when the Tit happened</param>
        public TitBasePlaceholder(DateTime date, long id = -1, string memo = null, long sum = 0, bool? cleared = null)
            : base(date, id, memo, sum, cleared)
        {
            Memo = "Content is loading…";
            Cleared = false;
        }

        #region Overrides of DataModelBase

        protected override void InsertToDb() {}

        public override bool ValidToInsert()
        {
            return false;
        }

        protected override void UpdateToDb() {}

        protected override void DeleteFromDb() {}

        #endregion
    }
}
