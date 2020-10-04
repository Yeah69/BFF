using System;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal sealed class SqliteParentTransactionRepository : SqliteRepositoryBase<IParentTransaction, ITransSql>
    {
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<ISqliteAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<ISqlitePayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<ISqliteSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<ISqliteFlagRepositoryInternal> _flagRepository;

        public SqliteParentTransactionRepository(
            ICrudOrm<ITransSql> crudOrm,
            Lazy<ISqliteAccountRepositoryInternal> accountRepository,
            Lazy<ISqlitePayeeRepositoryInternal> payeeRepository,
            Lazy<ISqliteSubTransactionRepository> subTransactionRepository,
            Lazy<ISqliteFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<IParentTransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            var parentTransaction = new Models.Domain.ParentTransaction(
                _crudOrm,
                _subTransactionRepository.Value,
                persistenceModel.Id,
                persistenceModel.Date,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.Value.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.Value.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.Value.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Cleared == 1L);

            foreach (var subTransaction in parentTransaction.SubTransactions)
            {
                subTransaction.Parent = parentTransaction;
            }

            return parentTransaction;
        }
    }
}