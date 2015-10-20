using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.SqLiteHelper;

namespace BFF.Model.Native
{
    class SubTransaction : DataModelBase, ITransactionLike
    {
        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Transaction Parent { get; set; }

        public long ParentId => Parent?.Id ?? -1;

        [Write(false)]
        public Category Category { get; set; }

        public long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = GetCategory(value); }
        }

        public string Memo { get; set; }
        
        public double Sum { get; set; }

        public static implicit operator SubTransaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
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
