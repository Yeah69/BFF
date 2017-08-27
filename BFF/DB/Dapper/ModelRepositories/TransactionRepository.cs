using System;
using System.Data.Common;
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
    
    public class TransactionRepository : RepositoryBase<Domain.ITransaction, Persistance.Transaction>
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;

        public TransactionRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
        }

        public override Domain.ITransaction Create() =>
            new Domain.Transaction(this, -1L, DateTime.Now, null, null, null, "", 0L, false);
        
        protected override Converter<Domain.ITransaction, Persistance.Transaction> ConvertToPersistance => domainTransaction => 
            new Persistance.Transaction
            {
                Id = domainTransaction.Id,
                AccountId = domainTransaction.Account.Id,
                CategoryId = domainTransaction.Category.Id,
                PayeeId = domainTransaction.Payee.Id,
                Date = domainTransaction.Date,
                Memo = domainTransaction.Memo,
                Sum = domainTransaction.Sum,
                Cleared = domainTransaction.Cleared ? 1L : 0L
            };

        protected override Converter<(Persistance.Transaction, DbConnection), Domain.ITransaction> ConvertToDomain => tuple =>
        {
            (Persistance.Transaction persistenceTransaction, DbConnection connection) = tuple;
            return new Domain.Transaction(
                this,
                persistenceTransaction.Id,
                persistenceTransaction.Date,
                _accountFetcher(persistenceTransaction.AccountId, connection),
                _payeeFetcher(persistenceTransaction.PayeeId, connection),
                _categoryFetcher(persistenceTransaction.CategoryId, connection),
                persistenceTransaction.Memo,
                persistenceTransaction.Sum,
                persistenceTransaction.Cleared == 1L);
        };
    }
}