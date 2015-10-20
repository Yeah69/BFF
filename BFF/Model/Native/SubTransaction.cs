using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using static BFF.DB.SQLite.Helper;

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
            set { Category = Category.GetFromDb(value); }
        }

        public string Memo { get; set; }
        
        public double Sum { get; set; }
        
        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(ParentId)} INTEGER,
                        {nameof(CategoryId)} INTEGER,
                        {nameof(Memo)} TEXT,
                        {nameof(Sum)} FLOAT,
                        FOREIGN KEY({nameof(ParentId)}) REFERENCES {nameof(Transaction)}s({nameof(Transaction.Id)}) ON DELETE CASCADE);";

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

        public static IEnumerable<SubTransaction> GetFromDb(long parentId)
        {
            IEnumerable<SubTransaction> ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{nameof(SubTransaction)}s] WHERE ParentId = @id;";
                ret =  cnn.Query<SubTransaction>(query, new {id = parentId});

                //ret = cnn.Get<Account>(id);

                cnn.Close();
            }
            return null;
        }
    }
}
