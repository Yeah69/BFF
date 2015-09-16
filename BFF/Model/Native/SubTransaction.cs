using BFF.Helper.Conversion;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    class SubTransaction : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Key]
        public override long Id { get; set; } = -1;

        [Write(false)]
        public Transaction Parent { get; set; }

        public long ParentId
        {
            get { return Parent?.Id ?? -1; }
            set { Parent.Id = value; }
        }

        [Write(false)]
        public Category Category { get; set; }

        public long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category.Id = value; }
        }

        public string Memo { get; set; }
        
        public double Outflow { get; set; }
        
        public double Inflow { get; set; }

        #endregion

        #region Methods

        #endregion

        #endregion

        #region Static

        #region Static Variables

        //todo: Make ParentId a foreign key
        //todo: Merge Outflow and Inflow into Sum
        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(ParentId)} INTEGER,
                        {nameof(CategoryId)} INTEGER,
                        {nameof(Memo)} TEXT,
                        {nameof(Outflow)} FLOAT,
                        {nameof(Inflow)} FLOAT);";

        #endregion

        #region Static Methods

        public static implicit operator SubTransaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = (ynabTransaction.SubCategory == string.Empty) ?
                Category.GetOrCreate(ynabTransaction.MasterCategory) :
                Category.GetOrCreate($"{ynabTransaction.MasterCategory};{ynabTransaction.SubCategory}");
            SubTransaction ret = new SubTransaction
            {
                Category = tempCategory,
                Memo = YnabConversion.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["subTransMemo"].Value,
                Outflow = ynabTransaction.Outflow,
                Inflow = ynabTransaction.Inflow
            };
            return ret;
        }

        #endregion

        #endregion
    }
}
