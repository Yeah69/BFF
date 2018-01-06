using System;
using System.Data.Common;
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
            IAccountRepository accountRepository,
            IFlagRepository flagRepository) : base(provideConnection)
        {
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<Domain.ITransfer, Trans> ConvertToPersistence => domainTransfer => 
            new Trans
            {
                Id = domainTransfer.Id,
                AccountId = -69,
                FlagId = domainTransfer.Flag == null || domainTransfer.Flag == Domain.Flag.Default ? (long?)null : domainTransfer.Flag.Id,
                CheckNumber = domainTransfer.CheckNumber,
                PayeeId = domainTransfer.FromAccount.Id,
                CategoryId = domainTransfer.ToAccount.Id,
                Date = domainTransfer.Date,
                Memo = domainTransfer.Memo,
                Sum = domainTransfer.Sum,
                Cleared = domainTransfer.Cleared ? 1L : 0L,
                Type = nameof(TransType.Transfer)
            };

        protected override Converter<(Trans, DbConnection), Domain.ITransfer> ConvertToDomain => tuple =>
        {
            (Trans persistenceTransfer, DbConnection connection) = tuple;
            return new Domain.Transfer(
                this,
                persistenceTransfer.Date,
                persistenceTransfer.Id,
                persistenceTransfer.FlagId == null ? null : _flagRepository.Find((long)persistenceTransfer.FlagId, connection),
                persistenceTransfer.CheckNumber,
                _accountRepository.Find(persistenceTransfer.PayeeId, connection),
                _accountRepository.Find(persistenceTransfer.CategoryId ?? -1, connection),  // This CategoryId should never be a null, because it comes from a transfer
                persistenceTransfer.Memo,
                persistenceTransfer.Sum,
                persistenceTransfer.Cleared == 1L);
        };
    }
}