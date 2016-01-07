using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class SubIncome : SubTitBase
    {

        [Write(false)]
        public override TitBase Parent { get; set; }

        public override long ParentId => Parent?.Id ?? -1;

        [Write(false)]
        public override Category Category { get; set; }

        public override long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = Database?.GetCategory(value); }
        }

        public override string Memo { get; set; }

        public override long Sum { get; set; }

        [Write(false)]
        public Type Type => typeof(SubIncome);
    }
}
