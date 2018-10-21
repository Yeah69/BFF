using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransferRepository : IRepositoryBase<ITransfer>
    {
    }

    internal sealed class TransferRepository : RepositoryBase<ITransfer, ITransDto>, ITransferRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly IFlagRepository _flagRepository;
        private readonly Func<ITransDto> _transDtoFactory;

        public TransferRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            IFlagRepository flagRepository,
            Func<ITransDto> transDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
            _transDtoFactory = transDtoFactory;
        }
        
        protected override Converter<ITransfer, ITransDto> ConvertToPersistence => domainTransfer =>
        {
            var transDto = _transDtoFactory();

            transDto.Id = domainTransfer.Id;
            transDto.AccountId = -69;
            transDto.FlagId = domainTransfer.Flag is null || domainTransfer.Flag == Flag.Default
                ? (long?)null
                : domainTransfer.Flag.Id;
            transDto.CheckNumber = domainTransfer.CheckNumber;
            transDto.PayeeId = domainTransfer.FromAccount?.Id;
            transDto.CategoryId = domainTransfer.ToAccount?.Id;
            transDto.Date = domainTransfer.Date;
            transDto.Memo = domainTransfer.Memo;
            transDto.Sum = domainTransfer.Sum;
            transDto.Cleared = domainTransfer.Cleared ? 1L : 0L;
            transDto.Type = nameof(TransType.Transfer);

            return transDto;
        };

        protected override async Task<ITransfer> ConvertToDomainAsync(ITransDto persistenceModel)
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