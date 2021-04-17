using System;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal sealed class SqliteTransferRepository : SqliteRepositoryBase<ITransfer, ITransSql>
    {
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<ISqliteAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<ISqliteFlagRepositoryInternal> _flagRepository;

        public SqliteTransferRepository(
            ICrudOrm<ITransSql> crudOrm,
            Lazy<ISqliteAccountRepositoryInternal> accountRepository,
            Lazy<ISqliteFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransfer> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            return 
                new Models.Domain.Transfer(
                    _crudOrm,
                    persistenceModel.Id,
                    persistenceModel.Date,
                    persistenceModel.FlagId is null
                        ? null
                        : await _flagRepository.Value.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                    persistenceModel.CheckNumber,
                    persistenceModel.PayeeId is null
                        ? null
                        : await _accountRepository.Value.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                    persistenceModel.CategoryId is null
                        ? null
                        : await _accountRepository.Value.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum,
                    persistenceModel.Cleared == 1L);
        }
    }
}