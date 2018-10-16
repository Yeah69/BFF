using System;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransferRepository : IRepositoryBase<ITransfer>
    {
    }

    public sealed class TransferRepository : RepositoryBase<ITransfer, TransDto>, ITransferRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly IFlagRepository _flagRepository;

        public TransferRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<ITransfer, TransDto> ConvertToPersistence => domainTransfer => 
            new TransDto
            {
                Id = domainTransfer.Id,
                AccountId = -69,
                FlagId = domainTransfer.Flag is null || domainTransfer.Flag == Flag.Default 
                    ? (long?)null 
                    : domainTransfer.Flag.Id,
                CheckNumber = domainTransfer.CheckNumber,
                PayeeId = domainTransfer.FromAccount?.Id,
                CategoryId = domainTransfer.ToAccount?.Id,
                Date = domainTransfer.Date,
                Memo = domainTransfer.Memo,
                Sum = domainTransfer.Sum,
                Cleared = domainTransfer.Cleared ? 1L : 0L,
                Type = nameof(TransType.Transfer)
            };

        protected override async Task<ITransfer> ConvertToDomainAsync(TransDto persistenceModel)
        {
            return 
                new Transfer(
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Date,
                    persistenceModel.Id,
                    persistenceModel.FlagId is null
                        ? null
                        : await _flagRepository.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                    persistenceModel.CheckNumber,
                    persistenceModel.PayeeId is null
                        ? null
                        : await _accountRepository.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                    persistenceModel.CategoryId is null
                        ? null
                        : await _accountRepository.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum,
                    persistenceModel.Cleared == 1L);
        }
    }
}