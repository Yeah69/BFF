using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Budget : DataModelBase
    {
        [Key]
        public override long Id { get; set; } = -1;

        public DateTime MonthYear { get; set; }

        //Todo: budget relevant properties
    }
}
