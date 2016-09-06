using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    internal interface IBudget : IDataModelBase
    {
        DateTime MonthYear { get; set; }
    }

    class Budget : DataModelBase, IBudget
    {
        public DateTime MonthYear { get; set; }

        //Todo: budget relevant properties

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Insert(this);
        }

        public override void Update(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Update(this);
        }

        public override void Delete(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Delete(this);
        }

        #endregion
    }
}
