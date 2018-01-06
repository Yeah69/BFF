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
            FOREIGN KEY({nameof(Trans.FlagId)}) REFERENCES {nameof(Flag)}s({nameof(Flag.Id)}) ON DELETE SET NULL);
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.AccountId)}_{nameof(Trans.PayeeId)}_index ON Transs ({nameof(Trans.AccountId)}, {nameof(Trans.PayeeId)});
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.AccountId)}_{nameof(Trans.CategoryId)}_index ON {nameof(Trans)}s ({nameof(Trans.AccountId)}, {nameof(Trans.CategoryId)});
            CREATE INDEX {nameof(Trans)}s_{nameof(Trans.Date)}_index ON {nameof(Trans)}s ({nameof(Trans.Date)});";      
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
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionsRepository;
        private readonly IFlagRepository _flagRepository;

        public TransRepository(
            IProvideConnection provideConnection, 
            IRepository<Domain.ITransaction> transactionRepository, 
            IRepository<Domain.ITransfer> transferRepository, 
            IRepository<Domain.IParentTransaction> parentTransactionRepository,
            IAccountRepository accountRepository,
            ICategoryBaseRepository categoryBaseRepository,
            IPayeeRepository payeeRepository,
            ISubTransactionRepository subTransactionsRepository, 
            IFlagRepository flagRepository)
            : base(provideConnection)
        {
            _provideConnection = provideConnection;
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _subTransactionsRepository = subTransactionsRepository;
            _flagRepository = flagRepository;
        }

        protected override Converter<(Trans, DbConnection), Domain.Structure.ITransBase> ConvertToDomain => tuple =>
        {
            (Trans trans, DbConnection connection) = tuple;
            Enum.TryParse(trans.Type, true, out Domain.Structure.TransType type);
            Domain.Structure.ITransBase ret;
            switch(type)
            {
                case Domain.Structure.TransType.Transaction:
                    ret = new Domain.Transaction(
                        _transactionRepository, 
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId, connection),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.AccountId, connection),
                        _payeeRepository.Find(trans.PayeeId, connection),
                        trans.CategoryId == null ? null : _categoryBaseRepository.Find((long)trans.CategoryId, connection), 
                        trans.Memo, 
                        trans.Sum,
                        trans.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId, connection),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.PayeeId, connection),
                        _accountRepository.Find(trans.CategoryId ?? -1, connection), // This CategoryId should never be a null, because it comes from a transfer
                        trans.Memo,
                        trans.Sum, 
                        trans.Cleared == 1L);
                    break;
                case Domain.Structure.TransType.ParentTransaction:
                    ret = new Domain.ParentTransaction(
                        _parentTransactionRepository,
                        _subTransactionsRepository.GetChildrenOf(trans.Id, connection), 
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId, connection),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.AccountId, connection),
                        _payeeRepository.Find(trans.PayeeId, connection),
                        trans.Memo, 
                        trans.Cleared == 1L);
                    break;
                default:
                    ret = new Domain.Transaction(_transactionRepository, DateTime.Today)
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
                return $"WHERE {nameof(Trans.AccountId)} = {specifyingObject.Id} OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.PayeeId)} = {specifyingObject.Id} OR {nameof(Trans.AccountId)} = -69 AND {nameof(Trans.CategoryId)} = {specifyingObject.Id}"; // TODO Replace specifyingObject.Id with @specifyingId

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