using System;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    class Budget : DataModelBase
    {
        public DateTime MonthYear { get; set; }

        //Todo: budget relevant properties
        protected override void InsertToDb()
        {
            Database?.Insert(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
        }

        protected override void DeleteFromDb()
        {
            Database?.Delete(this);
        }

        public override bool ValidToInsert()
        {
            return false; //... , because Budgets are not a feature, yet. todo: Update this when the time comes
        }
    }
}
