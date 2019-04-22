using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal sealed class SqliteTransactionRepository : SqliteRepositoryBase<ITransaction, ITransSql>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<ISqliteAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<ISqliteCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<ISqlitePayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<ISqliteFlagRepositoryInternal> _flagRepository;

        public SqliteTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            Lazy<ISqliteAccountRepositoryInternal> accountRepository,
            Lazy<ISqliteCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<ISqlitePayeeRepositoryInternal> payeeRepository,
            Lazy<ISqliteFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            return new Models.Domain.Transaction(
                _crudOrm,
                _rxSchedulerProvider,
                persistenceModel.Id,
                persistenceModel.Date,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.Value.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.Value.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.Value.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.Value.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum,
                persistenceModel.Cleared == 1L);
        }
    }
}