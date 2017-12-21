using System;
using System.Collections.Generic;
using System.Data.Common;
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
            ICategoryBase CategoryBaseFetcher(long? id, DbConnection connection) => id != null ? CategoryRepository.Find((long)id, connection) : null;
            ICategory CategoryFetcher(long? id, DbConnection connection) => id != null ? CategoryRepository.Find((long)id, connection) : null;
            IPayee PayeeFetcher(long id, DbConnection connection) => PayeeRepository.Find(id, connection);
            IEnumerable<ISubTransaction> SubTransactionsFetcher(long parentId, DbConnection connection) => SubTransactionRepository.GetChildrenOf(parentId, connection);
            IEnumerable<ISubIncome> SubIncomesFetcher(long parentId, DbConnection connection) => SubIncomeRepository.GetChildrenOf(parentId, connection);

            AccountRepository = new AccountRepository(provideConnection);
            BudgetEntryRepository = new BudgetEntryRepository(provideConnection, CategoryFetcher);
            CategoryRepository = new CategoryRepository(provideConnection);
            IncomeCategoryRepository = new IncomeCategoryRepository(provideConnection);
            DbSettingRepository = new DbSettingRepository(provideConnection);
            IncomeRepository = new IncomeRepository(provideConnection, AccountFetcher, CategoryBaseFetcher, PayeeFetcher);
            ParentIncomeRepository = new ParentIncomeRepository(provideConnection, AccountFetcher, PayeeFetcher, SubIncomesFetcher);
            ParentTransactionRepository = new ParentTransactionRepository(provideConnection, AccountFetcher, PayeeFetcher, SubTransactionsFetcher);
            PayeeRepository = new PayeeRepository(provideConnection);
            SubIncomeRepository = new SubIncomeRepository(provideConnection, CategoryBaseFetcher);
            SubTransactionRepository = new SubTransactionRepository(provideConnection, CategoryBaseFetcher);
            TransactionRepository = new TransactionRepository(provideConnection, AccountFetcher, CategoryBaseFetcher, PayeeFetcher);
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

            BudgetMonthRepository = new BudgetMonthRepository(BudgetEntryRepository, CategoryRepository, IncomeCategoryRepository, provideConnection);
        }

        public sealed override IAccountRepository AccountRepository { get; }
        public sealed override IBudgetEntryRepository BudgetEntryRepository { get; }
        public sealed override ICategoryRepository CategoryRepository { get; }
        public sealed override IIncomeCategoryRepository IncomeCategoryRepository { get; }
        public sealed override IDbSettingRepository DbSettingRepository { get; }
        public sealed override IIncomeRepository IncomeRepository { get; }
        public sealed override IParentIncomeRepository ParentIncomeRepository { get; }
        public sealed override IParentTransactionRepository ParentTransactionRepository { get; }
        public sealed override IPayeeRepository PayeeRepository { get; }
        public sealed override ISubIncomeRepository SubIncomeRepository { get; }
        public sealed override ISubTransactionRepository SubTransactionRepository { get; }
        public sealed override ITransactionRepository TransactionRepository { get; }
        public sealed override ITransferRepository TransferRepository { get; }
        public sealed override ITitRepository TitRepository { get; }
        public sealed override IBudgetMonthRepository BudgetMonthRepository { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AccountRepository?.Dispose();
                BudgetEntryRepository?.Dispose();
                CategoryRepository?.Dispose();
                DbSettingRepository?.Dispose();
                IncomeRepository?.Dispose();
                ParentIncomeRepository?.Dispose();
                ParentTransactionRepository?.Dispose();
                PayeeRepository?.Dispose();
                SubIncomeRepository?.Dispose();
                SubTransactionRepository?.Dispose();
                TransactionRepository?.Dispose();
                TransferRepository?.Dispose();
                TitRepository?.Dispose();
                BudgetMonthRepository?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}