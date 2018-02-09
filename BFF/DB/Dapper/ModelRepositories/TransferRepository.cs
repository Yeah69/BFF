using System;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransferRepository : IRepositoryBase<Domain.ITransfer>
    {
    }

    public sealed class TransferRepository : RepositoryBase<Domain.ITransfer, Trans>, ITransferRepository
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IFlagRepository _flagRepository;

        public TransferRepository(
            IProvideConnection provideConnection,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
        {
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

        protected override Converter<Trans, Domain.ITransfer> ConvertToDomain => persistenceTransfer => 
            new Domain.Transfer(
                this,
                persistenceTransfer.Date,
                persistenceTransfer.Id,
                persistenceTransfer.FlagId is null 
                    ? null
                    : _flagRepository.Find((long)persistenceTransfer.FlagId),
                persistenceTransfer.CheckNumber,
                persistenceTransfer.PayeeId is null 
                    ? null 
                    : _accountRepository.Find((long) persistenceTransfer.PayeeId),
                persistenceTransfer.CategoryId is null 
                    ? null 
                    : _accountRepository.Find((long) persistenceTransfer.CategoryId), 
                persistenceTransfer.Memo,
                persistenceTransfer.Sum,
                persistenceTransfer.Cleared == 1L);
    }
}