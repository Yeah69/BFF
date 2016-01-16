using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Budget : DataModelBase
    {
        public DateTime MonthYear { get; set; }

        //Todo: budget relevant properties
        protected override void DbUpdate()
        {
            Database?.Update(this);
        }
    }
}
