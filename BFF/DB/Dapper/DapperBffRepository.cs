using BFF.DB.Dapper.ModelRepositories;

namespace BFF.DB.Dapper
{
    public class DapperBffRepository : BffRepository
    {
        public DapperBffRepository(IProvideConnection provideConnection) 
            : base(provideConnection)
        {
            AccountRepository = new AccountRepository(provideConnection);
            BudgetEntryRepository = new BudgetEntryRepository(provideConnection);
            CategoryRepository = new CategoryRepository(provideConnection);
            DbSettingRepository = new DbSettingRepository(provideConnection);
            IncomeRepository = new IncomeRepository(provideConnection);
            ParentIncomeRepository = new ParentIncomeRepository(provideConnection);
            ParentTransactionRepository = new ParentTransactionRepository(provideConnection);
            PayeeRepository = new PayeeRepository(provideConnection);
            SubIncomeRepository = new SubIncomeRepository(provideConnection);
            SubTransactionRepository = new SubTransactionRepository(provideConnection);
            TransactionRepository = new TransactionRepository(provideConnection);
            TransferRepository = new TransferRepository(provideConnection);
            TitRepository = new TitRepository(provideConnection,
                                              TransactionRepository, 
                                              IncomeRepository,
                                              TransferRepository,
                                              ParentTransactionRepository,
                                              ParentIncomeRepository);
        }

        public sealed override AccountRepository AccountRepository { get; }
        public sealed override BudgetEntryRepository BudgetEntryRepository { get; }
        public sealed override CategoryRepository CategoryRepository { get; }
        public sealed override DbSettingRepository DbSettingRepository { get; }
        public sealed override IncomeRepository IncomeRepository { get; }
        public sealed override ParentIncomeRepository ParentIncomeRepository { get; }
        public sealed override ParentTransactionRepository ParentTransactionRepository { get; }
        public sealed override PayeeRepository PayeeRepository { get; }
        public sealed override SubIncomeRepository SubIncomeRepository { get; }
        public sealed override SubTransactionRepository SubTransactionRepository { get; }
        public sealed override TransactionRepository TransactionRepository { get; }
        public sealed override TransferRepository TransferRepository { get; }
        public sealed override TitRepository TitRepository { get; }
    }
}