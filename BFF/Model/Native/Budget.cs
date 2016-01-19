using System;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
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
    }
}
