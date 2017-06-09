using System.Data.Common;
using System.Transactions;
using BFF.DB.SQLite;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Transaction = BFF.MVVM.Models.Native.Transaction;

namespace BFF.DB
{
    public abstract class BffRepository
    {
        private IDbTableRepository<Account> _accountRepository;
        private IDbTableRepository<BudgetEntry> _budgetEntryRepository;
        private IDbTableRepository<Category> _categoryRepository;
        private IDbTableRepository<DbSetting> _dbSettingRepository;
        private IDbTableRepository<Income> _incomeRepository;
        private IDbTableRepository<ParentIncome> _parentIncomeRepository;
        private IDbTableRepository<ParentTransaction> _parentTransactionRepository;
        private IDbTableRepository<Payee> _payeeRepository;
        private IDbTableRepository<SubIncome> _subIncomeRepository;
        private IDbTableRepository<SubTransaction> _subTransactionRepository;
        private IDbTableRepository<Transaction> _transactionRepository;
        private IDbTableRepository<Transfer> _transferRepository;
        private IViewRepository<ITitBase, Account> _titRepository;

        private IProvideConnection _provideConnection;
        
        protected BffRepository(
            IDbTableRepository<Account> accountRepository, 
            IDbTableRepository<BudgetEntry> budgetEntryRepository, 
            IDbTableRepository<Category> categoryRepository, 
            IDbTableRepository<DbSetting> dbSettingRepository, 
            IDbTableRepository<Income> incomeRepository, 
            IDbTableRepository<ParentIncome> parentIncomeRepository, 
            IDbTableRepository<ParentTransaction> parentTransactionRepository, 
            IDbTableRepository<Payee> payeeRepository, 
            IDbTableRepository<SubIncome> subIncomeRepository, 
            IDbTableRepository<SubTransaction> subTransactionRepository, 
            IDbTableRepository<Transaction> transactionRepository, 
            IDbTableRepository<Transfer> transferRepository, 
            IViewRepository<ITitBase, Account> titRepository, 
            IProvideConnection provideConnection)
        {
            _accountRepository = accountRepository;
            _budgetEntryRepository = budgetEntryRepository;
            _categoryRepository = categoryRepository;
            _dbSettingRepository = dbSettingRepository;
            _incomeRepository = incomeRepository;
            _parentIncomeRepository = parentIncomeRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _payeeRepository = payeeRepository;
            _subIncomeRepository = subIncomeRepository;
            _subTransactionRepository = subTransactionRepository;
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _titRepository = titRepository;
            _provideConnection = provideConnection;
        }

        public IDbTableRepository<Account> AccountRepository => _accountRepository;
        public IDbTableRepository<BudgetEntry> BudgetEntryRepository => _budgetEntryRepository;
        public IDbTableRepository<Category> CategoryRepository => _categoryRepository;
        public IDbTableRepository<DbSetting> DbSettingRepository => _dbSettingRepository;
        public IDbTableRepository<Income> IncomeRepository => _incomeRepository;
        public IDbTableRepository<ParentIncome> ParentIncomeRepository => _parentIncomeRepository;
        public IDbTableRepository<ParentTransaction> ParentTransactionRepository => _parentTransactionRepository;
        public IDbTableRepository<Payee> PayeeRepository => _payeeRepository;
        public IDbTableRepository<SubIncome> SubIncomeRepository => _subIncomeRepository;
        public IDbTableRepository<SubTransaction> SubTransactionRepository => _subTransactionRepository;
        public IDbTableRepository<Transaction> TransactionRepository => _transactionRepository;
        public IDbTableRepository<Transfer> TransferRepository => _transferRepository;
        public IViewRepository<ITitBase, Account> TitRepository => _titRepository;

        private void CreateTablesInner(DbConnection connection)
        {
            _payeeRepository.CreateTable(connection);
            _categoryRepository.CreateTable(connection);
            _accountRepository.CreateTable(connection);

            _transactionRepository.CreateTable(connection);
            _parentTransactionRepository.CreateTable(connection);
            _subTransactionRepository.CreateTable(connection);

            _incomeRepository.CreateTable(connection);
            _parentIncomeRepository.CreateTable(connection);
            _subIncomeRepository.CreateTable(connection);

            _transferRepository.CreateTable(connection);

            _dbSettingRepository.CreateTable(connection);
            _dbSettingRepository.Add(new DbSetting(), connection);

            _budgetEntryRepository.CreateTable(connection);

            connection.Execute(SqLiteQueries.SetDatabaseSchemaVersion);

            connection.Execute(SqLiteQueries.CreateTheTitViewStatement);
        }
        
        public void CreateTables(DbConnection connection = null)
        {
            if(connection != null) CreateTablesInner(connection);
            else
            {
                using(TransactionScope transactionScope = new TransactionScope())
                using(DbConnection newConnection = _provideConnection.Connection)
                {
                    newConnection.Open();
                    CreateTablesInner(newConnection);
                    transactionScope.Complete();
                }
            }
        }
    }
}