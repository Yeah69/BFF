using System;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransferRepository : IRepositoryBase<Domain.ITransfer>
    {
    }

    public sealed class TransferRepository : RepositoryBase<Domain.ITransfer, Trans>, ITransferRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly INotifyBudgetOverviewRelevantChange _notifyBudgetOverviewRelevantChange;
        private readonly IAccountRepository _accountRepository;
        private readonly IFlagRepository _flagRepository;

        public TransferRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            INotifyBudgetOverviewRelevantChange notifyBudgetOverviewRelevantChange,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _notifyBudgetOverviewRelevantChange = notifyBudgetOverviewRelevantChange;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<Domain.ITransfer, Trans> ConvertToPersistence => domainTransfer => 
            new Trans
            {
                Id = domainTransfer.Id,
                AccountId = -69,
                FlagId = domainTransfer.Flag is null || domainTransfer.Flag == Domain.Flag.Default 
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

        protected override async Task<Domain.ITransfer> ConvertToDomainAsync(Trans persistenceModel)
        {
            return 
                new Domain.Transfer(
                    this,
                    _rxSchedulerProvider,
                    _notifyBudgetOverviewRelevantChange,
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