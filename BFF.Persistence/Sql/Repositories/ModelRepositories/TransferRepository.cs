using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface ITransferRepository
    {
    }

    internal sealed class TransferRepository : RepositoryBase<ITransfer, ITransSql>, ITransferRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<IAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IFlagRepositoryInternal> _flagRepository;

        public TransferRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            Lazy<IAccountRepositoryInternal> accountRepository,
            Lazy<IFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransfer> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            return 
                new Models.Domain.Transfer(
                    _crudOrm,
                    _rxSchedulerProvider,
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