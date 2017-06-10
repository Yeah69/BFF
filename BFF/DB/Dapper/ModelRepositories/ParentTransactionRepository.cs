using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentTransactionTable : CreateTableBase
    {
        public CreateParentTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.ParentTransaction)}s](
            {nameof(Persistance.ParentTransaction.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.ParentTransaction.AccountId)} INTEGER,
            {nameof(Persistance.ParentTransaction.PayeeId)} INTEGER,
            {nameof(Persistance.ParentTransaction.Date)} DATE,
            {nameof(Persistance.ParentTransaction.Memo)} TEXT,
            {nameof(Persistance.ParentTransaction.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.ParentTransaction.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.ParentTransaction.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class ParentTransactionRepository : RepositoryBase<Domain.ParentTransaction, Persistance.ParentTransaction>
    {
        public ParentTransactionRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.ParentTransaction, Persistance.ParentTransaction> ConvertToPersistance => domainParentTransaction => 
            new Persistance.ParentTransaction
            {
                Id = domainParentTransaction.Id,
                AccountId = domainParentTransaction.AccountId,
                PayeeId = domainParentTransaction.PayeeId,
                Date = domainParentTransaction.Date,
                Memo = domainParentTransaction.Memo,
                Cleared = domainParentTransaction.Cleared ? 1L : 0L
            };
        
        protected override Converter<Persistance.ParentTransaction, Domain.ParentTransaction> ConvertToDomain => persistanceParentTransaction =>
            new Domain.ParentTransaction(persistanceParentTransaction.Date)
            {
                Id = persistanceParentTransaction.Id,
                AccountId = persistanceParentTransaction.AccountId,
                PayeeId = persistanceParentTransaction.PayeeId,
                Date = persistanceParentTransaction.Date,
                Memo = persistanceParentTransaction.Memo,
                Cleared = persistanceParentTransaction.Cleared == 1L
            };
    }
}