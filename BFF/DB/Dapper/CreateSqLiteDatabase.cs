using System.Data.SQLite;
using System.IO;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.SQLite;

namespace BFF.DB.Dapper
{
    public class CreateSqLiteDatabase : CreateDatabaseBase
    {
        protected override ICreateTable CreateAccountTable { get; }
        protected override ICreateTable CreateBudgetEntryTable { get; }
        protected override ICreateTable CreateCategoryTable { get; }
        protected override ICreateTable CreateDbSettingTable { get; }
        protected override ICreateTable CreatePayeeTable { get; }
        protected override ICreateTable CreateFlagTable { get; }
        protected override ICreateTable CreateSubTransactionTable { get; }
        protected override ICreateTable CreateTransTable { get; }
        protected sealed override IProvideSqLiteConnetion ProvideConnection { get; }

        public CreateSqLiteDatabase(string fileName)
        {
            if (File.Exists(fileName)) //todo: This will make problems
                File.Delete(fileName);
            SQLiteConnection.CreateFile(fileName);
            
            ProvideConnection = new ProvideSqLiteConnection(fileName);
            
            CreateAccountTable = new CreateAccountTable(ProvideConnection);
            CreateBudgetEntryTable = new CreateBudgetEntryTable(ProvideConnection);
            CreateCategoryTable = new CreateCategoryTable(ProvideConnection);
            CreateDbSettingTable = new CreateDbSettingTable(ProvideConnection);
            CreatePayeeTable = new CreatePayeeTable(ProvideConnection);
            CreateFlagTable = new CreateFlagTable(ProvideConnection);
            CreateSubTransactionTable = new CreateSubTransactionTable(ProvideConnection);
            CreateTransTable = new CreateTransTable(ProvideConnection);
        }
    }
}