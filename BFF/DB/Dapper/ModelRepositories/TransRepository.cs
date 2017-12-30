using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTransTable : CreateTableBase
    {
        public CreateTransTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE {nameof(Trans)}s(
            {nameof(Trans.Id)} INTEGER PRIMARY KEY,
            {nameof(Trans.FlagId)} INTEGER,
            {nameof(Trans.CheckNumber)} TEXT,
            {nameof(Trans.AccountId)} INTEGER,
            {nameof(Trans.PayeeId)} INTEGER,
            {nameof(Trans.CategoryId)} INTEGER,
            {nameof(Trans.Date)} DATE,
            {nameof(Trans.Memo)} TEXT,
            {nameof(Trans.Sum)} INTEGER,
            {nameof(Trans.Cleared)} INTEGER,
            {nameof(Trans.Type)} VARCHAR(17),
            FOREIGN KEY({nameof(Trans.FlagId)}) REFERENCES {nameof(Flag)}s({nameof(Flag.Id)}) ON DELETE SET NULL);";      
    }

    public interface ITransRepository : 
        IRepositoryBase<Domain.Structure.ITransBase>, 
        ISpecifiedPagedAccess<Domain.Structure.ITransBase, Domain.IAccount>,
        ISpecifiedPagedAccessAsync<Domain.Structure.ITransBase, Domain.IAccount>
    {
    }


    public sealed class TransRepository : RepositoryBase<Domain.Structure.ITransBase, Trans>, ITransRepository
    {
        private readonly IProvideConnection _provideConnection;
        private readonly IRepository<Domain.ITransaction> _transactionRepository;
        private readonly IRepository<Domain.ITransfer> _transferRepository;
        private readonly IRepository<Domain.IParentTransaction> _parentTransactionRepository;
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long?, DbConnection, Domain.ICategoryBase> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> _subTransactionsFetcher;
        private readonly Func<long?, DbConnection, Domain.IFlag> _flagFetcher;

        public TransRepository(
            IProvideConnection provideConnection, 
            IRepository<Domain.ITransaction> transactionRepository, 
            IRepository<Domain.ITransfer> transferRepository, 
            IRepository<Domain.IParentTransaction> parentTransactionRepository,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long?, DbConnection, Domain.ICategoryBase> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher,
            Func<long, DbConnection, IEnumerable<Domain.ISubTransaction>> subTransactionsFetcher, 
            Func<long?, DbConnection, Domain.IFlag> flagFetcher)
            : base(provideConnection)
        {
            _provideConnection = provideConnection;
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
            _subTransactionsFetcher = subTransactionsFetcher;
            _flagFetcher = flagFetcher;
        }

        public override Domain.Structure.ITransBase Create() =>
            null;

        protected override Converter<(Trans, DbConnection), Domain.Structure.ITransBase> ConvertToDomain => tuple =>
        {
            (Trans theTit, DbConnection connection) = tuple;
            Enum.TryParse(theTit.Type, true, out Domain.Structure.TransType type);
            Domain.Structure.ITransBase ret;
            switch(type)
            {
                case Domain.Structure.TransType.Transaction:
                    ret = new Domain.Transaction(
                        _transactionRepository, 
                        theTit.Id,
                        _flagFetcher(theTit.FlagId, connection),
                        theTit.CheckNumber,
                        theTit.Date,
                        _accountFetcher(theTit.AccountId, connection),
                        _payeeFetcher(theTit.PayeeId, connection),
                        _categoryFetcher(theTit.CategoryId, connection), 
                        theTit.Memo, 
                        theTit.Sum,
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        theTit.Id,
                        _flagFetcher(theTit.FlagId, connection),
                        theTit.CheckNumber,
                        theTit.Date,
                        _accountFetcher(theTit.PayeeId, connection),
                        _accountFetcher(theTit.CategoryId ?? -1, connection), // This CategoryId should never be a null, because it comes from a transfer
                        theTit.Memo,
                        theTit.Sum, 
                        theTit.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.ParentTransaction:
                    ret = new Domain.ParentTransaction(
                        _parentTransactionRepository,
                        _subTransactionsFetcher(theTit.Id, connection), 
                        theTit.Id,
                        _flagFetcher(theTit.FlagId, connection),
                        theTit.CheckNumber,
                        theTit.Date,
                        _accountFetcher(theTit.AccountId, connection),
                        _payeeFetcher(theTit.PayeeId, connection),
                        theTit.Memo, 
                        theTit.Cleared == 1L);
                    break;
                default:
                    ret = new Domain.Transaction(_transactionRepository, -1L, null, "", DateTime.Today)
                        {Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR"};
                    break;
            }

            return ret;
        };

        protected override Converter<Domain.Structure.ITransBase, Trans> ConvertToPersistence => domainTransBase =>
        {
            long accountId = -69;
            long? categoryId = -69;
            long payeeId = -1;
            long sum = -69;
            string type = "";
            switch (domainTransBase)
            {
                case Domain.ITransaction transaction:
                    accountId = transaction.Account.Id;
                    categoryId = transaction.Category?.Id;
                    payeeId = transaction.Payee.Id;
                    sum = transaction.Sum;
                    type = TransType.Transaction.ToString();
                    break;
                case Domain.IParentTransaction parentTransaction:
                    accountId = parentTransaction.Account.Id;
                    payeeId = parentTransaction.Payee.Id;
                    type = TransType.ParentTransaction.ToString();
                    break;
                case Domain.ITransfer transfer:
                    categoryId = transfer.ToAccount.Id;
                    payeeId = transfer.FromAccount.Id;
                    sum = transfer.Sum;
                    type = TransType.Transfer.ToString();
                    break;
            }

            return new Trans
            {
                Id = domainTransBase.Id,
                AccountId = accountId,
                FlagId = domainTransBase.Flag == null || domainTransBase.Flag == Domain.Flag.Default
                    ? (long?) null
                    : domainTransBase.Flag.Id,
                CheckNumber = domainTransBase.CheckNumber,
                CategoryId = categoryId,
                PayeeId = payeeId,
                Date = domainTransBase.Date,
                Memo = domainTransBase.Memo,
                Sum = sum,
                Cleared = domainTransBase.Cleared ? 1L : 0L,
                Type = type
            };
        };

        private string GetOrderingPageSuffix() => $"ORDER BY {nameof(Trans.Date)}";

        private string CommonSuffix(Domain.IAccount specifyingObject)
        {
            if (specifyingObject != null && !(specifyingObject is Domain.ISummaryAccount))
                return $"WHERE {nameof(Trans.AccountId)} = {specifyingObject.Id} OR {nameof(Trans.AccountId)} = -69 AND ({nameof(Trans.PayeeId)} = {specifyingObject.Id} OR {nameof(Trans.CategoryId)} = {specifyingObject.Id})"; // TODO Replace specifyingObject.Id with @specifyingId

            return "";
        }

        private string GetSpecifyingPageSuffix(Domain.IAccount specifyingObject) => CommonSuffix(specifyingObject);

        private string GetSpecifyingCountSuffix(Domain.IAccount specifyingObject) => CommonSuffix(specifyingObject);

        public IEnumerable<ITransBase> GetPage(int offset, int pageSize, Domain.IAccount specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT * FROM {nameof(Trans)}s {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix()} LIMIT @{nameof(offset)}, @{nameof(pageSize)};";

            return ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<Trans>(query, new {offset, pageSize}).Select(tt => ConvertToDomain((tt, c))),
                _provideConnection,
                connection);
        }

        public int GetCount(Domain.IAccount specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM {nameof(Trans)}s {GetSpecifyingCountSuffix(specifyingObject)};";

            return ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<int>(query),
                _provideConnection,
                connection).First();
        }

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, Domain.IAccount specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT * FROM {nameof(Trans)}s {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix()} LIMIT @{nameof(offset)}, @{nameof(pageSize)};";

            return await ConnectionHelper.QueryOnExistingOrNewConnectionAsync(
                async c => (await c.QueryAsync<Trans>(query, new {offset, pageSize })).Select(tt => ConvertToDomain((tt, c))),
                _provideConnection,
                connection);
        }

        public async Task<int> GetCountAsync(Domain.IAccount specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM {nameof(Trans)}s {GetSpecifyingCountSuffix(specifyingObject)};";

            return (await ConnectionHelper.QueryOnExistingOrNewConnectionAsync(
                async c => await c.QueryAsync<int>(query),
                _provideConnection,
                connection)).First();
        }
    }
}