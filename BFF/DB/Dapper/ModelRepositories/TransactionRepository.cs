using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTransactionTable : CreateTableBase
    {
        public CreateTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Transaction)}s](
            {nameof(Persistance.Transaction.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Transaction.AccountId)} INTEGER,
            {nameof(Persistance.Transaction.PayeeId)} INTEGER,
            {nameof(Persistance.Transaction.CategoryId)} INTEGER,
            {nameof(Persistance.Transaction.Date)} DATE,
            {nameof(Persistance.Transaction.Memo)} TEXT,
            {nameof(Persistance.Transaction.Sum)} INTEGER,
            {nameof(Persistance.Transaction.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.Transaction.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.Transaction.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL,
            FOREIGN KEY({nameof(Persistance.Transaction.CategoryId)}) REFERENCES {nameof(Persistance.Category)}s({nameof(Persistance.Category.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class TransactionRepository : RepositoryBase<Domain.Transaction, Persistance.Transaction>
    {
        public TransactionRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.Transaction, Persistance.Transaction> ConvertToPersistance => domainTransaction => 
            new Persistance.Transaction
            {
                Id = domainTransaction.Id,
                AccountId = domainTransaction.AccountId,
                CategoryId = domainTransaction.CategoryId,
                PayeeId = domainTransaction.PayeeId,
                Date = domainTransaction.Date,
                Memo = domainTransaction.Memo,
                Sum = domainTransaction.Sum,
                Cleared = domainTransaction.Cleared ? 1L : 0L
            };
        
        protected override Converter<Persistance.Transaction, Domain.Transaction> ConvertToDomain => persistanceTransaction =>
            new Domain.Transaction(persistanceTransaction.Date)
            {
                Id = persistanceTransaction.Id,
                AccountId = persistanceTransaction.AccountId,
                CategoryId = persistanceTransaction.CategoryId,
                PayeeId = persistanceTransaction.PayeeId,
                Date = persistanceTransaction.Date,
                Memo = persistanceTransaction.Memo,
                Sum = persistanceTransaction.Sum,
                Cleared = persistanceTransaction.Cleared == 1L
            };
    }
}