using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class SubTransaction : SubTransInc
    {
        [Key]
        public long Id { get; set; } = -1;

        [Write(false)]
        public override TitBase Parent { get; set; }

        public override long ParentId => Parent?.Id ?? -1;

        [Write(false)]
        public override Category Category { get; set; }

        public override long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = GetCategory(value); }
        }

        public override string Memo { get; set; }
        
        public override long Sum { get; set; }

        [Write(false)]
        public Type Type => typeof(SubTransaction);

        public static implicit operator SubTransaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            SubTransaction ret = new SubTransaction
            {
                Category = tempCategory,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["subTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
            };
            return ret;
        }
    }
}
