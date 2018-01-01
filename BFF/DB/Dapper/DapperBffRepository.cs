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
            IAccount AccountFetcher(long id, DbConnection connection) 
                => AccountRepository.Find(id, connection);
            ICategoryBase CategoryBaseFetcher(long? id, DbConnection connection) 
                => id != null ? CategoryBaseRepository.Find((long)id, connection) : null;
            ICategory CategoryFetcher(long? id, DbConnection connection) 
                => id != null ? CategoryRepository.Find((long)id, connection) : null;
            IPayee PayeeFetcher(long id, DbConnection connection)
                => PayeeRepository.Find(id, connection);
            IFlag FlagFetcher(long? id, DbConnection connection) 
                => id != null ? FlagRepository.Find((long) id, connection) : Flag.Default;
            IEnumerable<ISubTransaction> SubTransactionsFetcher(long parentId, DbConnection connection)
                => SubTransactionRepository.GetChildrenOf(parentId, connection);

            AccountRepository = new AccountRepository(provideConnection);
            BudgetEntryRepository = new BudgetEntryRepository(provideConnection, CategoryFetcher);
            CategoryRepository = new CategoryRepository(provideConnection);
            IncomeCategoryRepository = new IncomeCategoryRepository(provideConnection);
            CategoryBaseRepository = new CategoryBaseRepository(CategoryRepository, IncomeCategoryRepository);
            FlagRepository = new FlagRepository(provideConnection);
            DbSettingRepository = new DbSettingRepository(provideConnection);
            ParentTransactionRepository = new ParentTransactionRepository(provideConnection, AccountFetcher, PayeeFetcher, SubTransactionsFetcher, FlagFetcher);
            PayeeRepository = new PayeeRepository(provideConnection);
            SubTransactionRepository = new SubTransactionRepository(provideConnection, CategoryBaseFetcher);
            TransactionRepository = new TransactionRepository(provideConnection, AccountFetcher, CategoryBaseFetcher, PayeeFetcher, FlagFetcher);
            TransferRepository = new TransferRepository(provideConnection, AccountFetcher, FlagFetcher);
            TransRepository = new TransRepository(
                provideConnection,
                TransactionRepository,
                TransferRepository,
                ParentTransactionRepository,
                AccountFetcher,
                CategoryBaseFetcher,
                PayeeFetcher,
                SubTransactionsFetcher,
                FlagFetcher);

            BudgetMonthRepository = new BudgetMonthRepository(BudgetEntryRepository, CategoryRepository, IncomeCategoryRepository, AccountRepository, provideConnection);
        }

        public sealed override IAccountRepository AccountRepository { get; }
        public sealed override IBudgetEntryRepository BudgetEntryRepository { get; }
        public sealed override ICategoryRepository CategoryRepository { get; }
        public sealed override IIncomeCategoryRepository IncomeCategoryRepository { get; }
        public override ICategoryBaseRepository CategoryBaseRepository { get; }
        public sealed override IDbSettingRepository DbSettingRepository { get; }
        public sealed override IParentTransactionRepository ParentTransactionRepository { get; }
        public sealed override IPayeeRepository PayeeRepository { get; }
        public sealed override ISubTransactionRepository SubTransactionRepository { get; }
        public sealed override ITransactionRepository TransactionRepository { get; }
        public override ITransRepository TransRepository { get; }
        public sealed override ITransferRepository TransferRepository { get; }
        public sealed override IBudgetMonthRepository BudgetMonthRepository { get; }
        public override IFlagRepository FlagRepository { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AccountRepository?.Dispose();
                BudgetEntryRepository?.Dispose();
                CategoryRepository?.Dispose();
                DbSettingRepository?.Dispose();
                ParentTransactionRepository?.Dispose();
                PayeeRepository?.Dispose();
                SubTransactionRepository?.Dispose();
                TransactionRepository?.Dispose();
                TransferRepository?.Dispose();
                BudgetMonthRepository?.Dispose();
                FlagRepository?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}