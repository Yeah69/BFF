using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper
{
    public class DapperBffRepository : BffRepository
    {
        public DapperBffRepository(IProvideConnection provideConnection) 
            : base(provideConnection)
        {
            IAccount AccountFetcher(long id, DbConnection connection) => AccountRepository.Find(id, connection);
            ICategory CategoryFetcher(long id, DbConnection connection) => CategoryRepository.Find(id, connection);
            IPayee PayeeFetcher(long id, DbConnection connection) => PayeeRepository.Find(id, connection);
            IEnumerable<ISubTransaction> SubTransactionsFetcher(long parentId, DbConnection connection) => SubTransactionRepository.GetChildrenOf(parentId, connection);
            IEnumerable<ISubIncome> SubIncomesFetcher(long parentId, DbConnection connection) => SubIncomeRepository.GetChildrenOf(parentId, connection);
            IParentTransaction ParentTransactionFetcher(long id, DbConnection connection) => ParentTransactionRepository.Find(id, connection);
            IParentIncome ParentIncomeFetcher(long id, DbConnection connection) => ParentIncomeRepository.Find(id, connection);

            AccountRepository = new AccountRepository(provideConnection);
            BudgetEntryRepository = new BudgetEntryRepository(provideConnection, CategoryFetcher);
            CategoryRepository = new CategoryRepository(provideConnection);
            DbSettingRepository = new DbSettingRepository(provideConnection);
            IncomeRepository = new IncomeRepository(provideConnection, AccountFetcher, CategoryFetcher, PayeeFetcher);
            ParentIncomeRepository = new ParentIncomeRepository(provideConnection, AccountFetcher, PayeeFetcher, SubIncomesFetcher);
            ParentTransactionRepository = new ParentTransactionRepository(provideConnection, AccountFetcher, PayeeFetcher, SubTransactionsFetcher);
            PayeeRepository = new PayeeRepository(provideConnection);
            SubIncomeRepository = new SubIncomeRepository(provideConnection, ParentIncomeFetcher, CategoryFetcher);
            SubTransactionRepository = new SubTransactionRepository(provideConnection, ParentTransactionFetcher, CategoryFetcher);
            TransactionRepository = new TransactionRepository(provideConnection, AccountFetcher, CategoryFetcher, PayeeFetcher);
            TransferRepository = new TransferRepository(provideConnection, AccountFetcher);
            TitRepository = new TitRepository(provideConnection,
                                              TransactionRepository, 
                                              IncomeRepository,
                                              TransferRepository,
                                              ParentTransactionRepository,
                                              ParentIncomeRepository,
                                              AccountFetcher, 
                                              CategoryFetcher, 
                                              PayeeFetcher, 
                                              SubTransactionsFetcher,
                                              SubIncomesFetcher);
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