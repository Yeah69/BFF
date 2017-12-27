using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTransferTable : CreateTableBase
    {
        public CreateTransferTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Transfer)}s](
            {nameof(Transfer.Id)} INTEGER PRIMARY KEY,
            {nameof(Transfer.FlagId)} INTEGER,
            {nameof(Transfer.CheckNumber)} TEXT,
            {nameof(Transfer.FromAccountId)} INTEGER,
            {nameof(Transfer.ToAccountId)} INTEGER,
            {nameof(Transfer.Date)} DATE,
            {nameof(Transfer.Memo)} TEXT,
            {nameof(Transfer.Sum)} INTEGER,
            {nameof(Transfer.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Transfer.FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT,
            FOREIGN KEY({nameof(Transfer.ToAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT,
            FOREIGN KEY({nameof(Transfer.FlagId)}) REFERENCES {nameof(Flag)}s({nameof(Flag.Id)}) ON DELETE SET NULL);";

    }

    public interface ITransferRepository : IRepositoryBase<Domain.ITransfer>
    {
    }

    public sealed class TransferRepository : RepositoryBase<Domain.ITransfer, Transfer>, ITransferRepository
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
        
        protected override Converter<Domain.ITransfer, Transfer> ConvertToPersistence => domainTransfer => 
            new Transfer
            {
                Id = domainTransfer.Id,
                FlagId = domainTransfer.Flag == null || domainTransfer.Flag == Domain.Flag.Default ? (long?)null : domainTransfer.Flag.Id,
                CheckNumber = domainTransfer.CheckNumber,
                FromAccountId = domainTransfer.FromAccount.Id,
                ToAccountId = domainTransfer.ToAccount.Id,
                Date = domainTransfer.Date,
                Memo = domainTransfer.Memo,
                Sum = domainTransfer.Sum,
                Cleared = domainTransfer.Cleared ? 1L : 0L
            };

        protected override Converter<(Transfer, DbConnection), Domain.ITransfer> ConvertToDomain => tuple =>
        {
            (Transfer persistenceTransfer, DbConnection connection) = tuple;
            return new Domain.Transfer(
                this,
                persistenceTransfer.Id,
                _flagFetcher(persistenceTransfer.Id, connection),
                persistenceTransfer.CheckNumber,
                persistenceTransfer.Date,
                _accountFetcher(persistenceTransfer.FromAccountId, connection),
                _accountFetcher(persistenceTransfer.ToAccountId, connection),
                persistenceTransfer.Memo,
                persistenceTransfer.Sum,
                persistenceTransfer.Cleared == 1L);
        };
    }
}