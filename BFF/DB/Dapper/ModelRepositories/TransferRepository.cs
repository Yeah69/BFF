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
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long?, DbConnection, Domain.IFlag> _flagFetcher;

        public TransferRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long?, DbConnection, Domain.IFlag> flagFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _flagFetcher = flagFetcher;
        }

        public override Domain.ITransfer Create() =>
            new Domain.Transfer(this, -1L, null, "", DateTime.MinValue, null, null, "", 0L);
        
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
                persistenceTransfer.Id,
                _flagFetcher(persistenceTransfer.Id, connection),
                persistenceTransfer.CheckNumber,
                persistenceTransfer.Date,
                _accountFetcher(persistenceTransfer.PayeeId, connection),
                _accountFetcher(persistenceTransfer.CategoryId ?? -1, connection),  // This CategoryId should never be a null, because it comes from a transfer
                persistenceTransfer.Memo,
                persistenceTransfer.Sum,
                persistenceTransfer.Cleared == 1L);
        };
    }
}