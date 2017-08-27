using System;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTransferTable : CreateTableBase
    {
        public CreateTransferTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Transfer)}s](
            {nameof(Persistance.Transfer.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Transfer.FromAccountId)} INTEGER,
            {nameof(Persistance.Transfer.ToAccountId)} INTEGER,
            {nameof(Persistance.Transfer.Date)} DATE,
            {nameof(Persistance.Transfer.Memo)} TEXT,
            {nameof(Persistance.Transfer.Sum)} INTEGER,
            {nameof(Persistance.Transfer.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.Transfer.FromAccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE RESTRICT,
            FOREIGN KEY({nameof(Persistance.Transfer.ToAccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE RESTRICT);";
        
    }
    
    public class TransferRepository : RepositoryBase<Domain.ITransfer, Persistance.Transfer>
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;

        public TransferRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
        }

        public override Domain.ITransfer Create() =>
            new Domain.Transfer(this, -1L, DateTime.MinValue, null, null, "", 0L);
        
        protected override Converter<Domain.ITransfer, Persistance.Transfer> ConvertToPersistance => domainTransfer => 
            new Persistance.Transfer
            {
                Id = domainTransfer.Id,
                FromAccountId = domainTransfer.FromAccount.Id,
                ToAccountId = domainTransfer.ToAccount.Id,
                Date = domainTransfer.Date,
                Memo = domainTransfer.Memo,
                Sum = domainTransfer.Sum,
                Cleared = domainTransfer.Cleared ? 1L : 0L
            };

        protected override Converter<(Persistance.Transfer, DbConnection), Domain.ITransfer> ConvertToDomain => tuple =>
        {
            (Persistance.Transfer persistenceTransfer, DbConnection connection) = tuple;
            return new Domain.Transfer(
                this,
                persistenceTransfer.Id,
                persistenceTransfer.Date,
                _accountFetcher(persistenceTransfer.FromAccountId, connection),
                _accountFetcher(persistenceTransfer.ToAccountId, connection),
                persistenceTransfer.Memo,
                persistenceTransfer.Sum,
                persistenceTransfer.Cleared == 1L);
        };
    }
}