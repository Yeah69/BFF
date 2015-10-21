using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class SubIncome : SubTransInc, ITransactionLike
    {
        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public override TransactionIncome Parent { get; set; }

        public override long ParentId => Parent?.Id ?? -1;

        [Write(false)]
        public override Category Category { get; set; }

        public override long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = GetCategory(value); }
        }

        public override string Memo { get; set; }

        public override double Sum { get; set; }

        public Type Type => typeof(SubIncome);
    }
}
