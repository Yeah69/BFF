using BFF.MVVM.Models.Native;

namespace BFF.DB
{
    public abstract class BffRepository
    {
        public abstract IRepository<Account> AccountRepository { get; }
        public abstract IRepository<BudgetEntry> BudgetEntryRepository { get; }
        public abstract IRepository<Category> CategoryRepository { get; }
        public abstract IRepository<DbSetting> DbSettingRepository { get; }
        public abstract IRepository<Income> IncomeRepository { get; }
        public abstract IRepository<ParentIncome> ParentIncomeRepository { get; }
        public abstract IRepository<ParentTransaction> ParentTransactionRepository { get; }
        public abstract IRepository<Payee> PayeeRepository { get; }
        public abstract IRepository<SubIncome> SubIncomeRepository { get; }
        public abstract IRepository<SubTransaction> SubTransactionRepository { get; }
        public abstract IRepository<Transaction> TransactionRepository { get; }
        public abstract IRepository<Transfer> TransferRepository { get; }
    }
}