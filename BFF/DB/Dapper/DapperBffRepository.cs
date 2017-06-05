using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper
{
    public class DapperBffRepository : BffRepository
    {
        public override IRepository<Account> AccountRepository { get; }
        public override IRepository<BudgetEntry> BudgetEntryRepository { get; }
        public override IRepository<Category> CategoryRepository { get; }
        public override IRepository<DbSetting> DbSettingRepository { get; }
        public override IRepository<Income> IncomeRepository { get; }
        public override IRepository<ParentIncome> ParentIncomeRepository { get; }
        public override IRepository<ParentTransaction> ParentTransactionRepository { get; }
        public override IRepository<Payee> PayeeRepository { get; }
        public override IRepository<SubIncome> SubIncomeRepository { get; }
        public override IRepository<SubTransaction> SubTransactionRepository { get; }
        public override IRepository<Transaction> TransactionRepository { get; }
        public override IRepository<Transfer> TransferRepository { get; }

        public DapperBffRepository(IProvideConnection provideConnection)
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
        }
    }
}