using System.Data.SQLite;
using System.IO;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.SQLite;

namespace BFF.DB.Dapper
{
    public class CreateSqLiteDatebase : CreateDatabaseBase
    {
        protected override ICreateTable CreateAccountTable { get; }
        protected override ICreateTable CreateBudgetEntryTable { get; }
        protected override ICreateTable CreateCategoryTable { get; }
        protected override ICreateTable CreateDbSettingTable { get; }
        protected override ICreateTable CreateIncomeTable { get; }
        protected override ICreateTable CreateParentIncomeTable { get; }
        protected override ICreateTable CreateParentTransactionTable { get; }
        protected override ICreateTable CreatePayeeTable { get; }
        protected override ICreateTable CreateSubIncomeTable { get; }
        protected override ICreateTable CreateSubTransactionTable { get; }
        protected override ICreateTable CreateTransactionTable { get; }
        protected override ICreateTable CreateTransferTable { get; }
        protected override ICreateTable CreateTitTable { get; }
        protected sealed override IProvideConnection ProvideConnection { get; }

        public CreateSqLiteDatebase(string fileName)
        {
            if (File.Exists(fileName)) //todo: This will make problems
                File.Delete(fileName);
            SQLiteConnection.CreateFile(fileName);
            
            ProvideConnection = new ProvideSqLiteConnection(fileName);
            
            CreateAccountTable = new CreateAccountTable(ProvideConnection);
            CreateBudgetEntryTable = new CreateBudgetEntryTable(ProvideConnection);
            CreateCategoryTable = new CreateCategoryTable(ProvideConnection);
            CreateDbSettingTable = new CreateDbSettingTable(ProvideConnection);
            CreateIncomeTable = new CreateIncomeTable(ProvideConnection);
            CreateParentIncomeTable = new CreateParentIncomeTable(ProvideConnection);
            CreateParentTransactionTable = new CreateParentTransactionTable(ProvideConnection);
            CreatePayeeTable = new CreatePayeeTable(ProvideConnection);
            CreateSubIncomeTable = new CreateSubIncomeTable(ProvideConnection);
            CreateSubTransactionTable = new CreateSubTransactionTable(ProvideConnection);
            CreateTransactionTable = new CreateTransactionTable(ProvideConnection);
            CreateTransferTable = new CreateTransferTable(ProvideConnection);
            CreateTitTable = new CreateTitTable(ProvideConnection);
        }
    }
}