using System;
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
    
    public class TransferRepository : RepositoryBase<Domain.Transfer, Persistance.Transfer>
    {
        public TransferRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.Transfer, Persistance.Transfer> ConvertToPersistance => domainTransfer => 
            new Persistance.Transfer
            {
                Id = domainTransfer.Id,
                FromAccountId = domainTransfer.FromAccountId,
                ToAccountId = domainTransfer.ToAccountId,
                Date = domainTransfer.Date,
                Memo = domainTransfer.Memo,
                Sum = domainTransfer.Sum,
                Cleared = domainTransfer.Cleared ? 1L : 0L
            };
        
        protected override Converter<Persistance.Transfer, Domain.Transfer> ConvertToDomain => persistanceTransfer =>
            new Domain.Transfer(persistanceTransfer.Date)
            {
                Id = persistanceTransfer.Id,
                FromAccountId = persistanceTransfer.FromAccountId,
                ToAccountId = persistanceTransfer.ToAccountId,
                Date = persistanceTransfer.Date,
                Memo = persistanceTransfer.Memo,
                Sum = persistanceTransfer.Sum,
                Cleared = persistanceTransfer.Cleared == 1L 
            };
    }
}