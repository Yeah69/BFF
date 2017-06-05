using BFF.DB.Dapper.ModelRepositories;

namespace BFF.DB.Dapper
{
    public class DapperBffRepository : BffRepository
    {
        public DapperBffRepository(IProvideConnection provideConnection) 
            : base(new AccountRepository(provideConnection),
                   new BudgetEntryRepository(provideConnection),
                   new CategoryRepository(provideConnection), 
                   new DbSettingRepository(provideConnection),
                   new IncomeRepository(provideConnection),
                   new ParentIncomeRepository(provideConnection),
                   new ParentTransactionRepository(provideConnection),
                   new PayeeRepository(provideConnection),
                   new SubIncomeRepository(provideConnection),
                   new SubTransactionRepository(provideConnection),
                   new TransactionRepository(provideConnection),
                   new TransferRepository(provideConnection),
                   provideConnection)
        {
        }
    }
}