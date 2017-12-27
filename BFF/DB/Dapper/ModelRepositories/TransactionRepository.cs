using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateTransactionTable : CreateTableBase
    {
        public CreateTransactionTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Transaction)}s](
            {nameof(Transaction.Id)} INTEGER PRIMARY KEY,
            {nameof(Transaction.FlagId)} INTEGER,
            {nameof(Transaction.CheckNumber)} TEXT,
            {nameof(Transaction.AccountId)} INTEGER,
            {nameof(Transaction.PayeeId)} INTEGER,
            {nameof(Transaction.CategoryId)} INTEGER,
            {nameof(Transaction.Date)} DATE,
            {nameof(Transaction.Memo)} TEXT,
            {nameof(Transaction.Sum)} INTEGER,
            {nameof(Transaction.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Transaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Transaction.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL,
            FOREIGN KEY({nameof(Transaction.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL,
            FOREIGN KEY({nameof(Transaction.FlagId)}) REFERENCES {nameof(Flag)}s({nameof(Flag.Id)}) ON DELETE SET NULL);";

    }

    public interface ITransactionRepository : IRepositoryBase<Domain.ITransaction>
    {
    }

    public sealed class TransactionRepository : RepositoryBase<Domain.ITransaction, Transaction>, ITransactionRepository
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long?, DbConnection, Domain.ICategoryBase> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;
        private readonly Func<long?, DbConnection, Domain.IFlag> _flagFetcher;

        public TransactionRepository(
            IProvideConnection provideConnection,
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long?, DbConnection, Domain.ICategoryBase> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher, 
            Func<long?, DbConnection, Domain.IFlag> flagFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
            _flagFetcher = flagFetcher;
        }

        public override Domain.ITransaction Create() =>
            new Domain.Transaction(this, -1L, null, "", DateTime.Now, null, null, null, "", 0L, false);
        
        protected override Converter<Domain.ITransaction, Transaction> ConvertToPersistence => domainTransaction => 
            new Transaction
            {
                Id = domainTransaction.Id,
                AccountId = domainTransaction.Account.Id,
                FlagId = domainTransaction.Flag == null || domainTransaction.Flag == Domain.Flag.Default ? (long?)null : domainTransaction.Flag.Id,
                CheckNumber = domainTransaction.CheckNumber,
                CategoryId = domainTransaction.Category?.Id,
                PayeeId = domainTransaction.Payee.Id,
                Date = domainTransaction.Date,
                Memo = domainTransaction.Memo,
                Sum = domainTransaction.Sum,
                Cleared = domainTransaction.Cleared ? 1L : 0L
            };

        protected override Converter<(Transaction, DbConnection), Domain.ITransaction> ConvertToDomain => tuple =>
        {
            (Transaction persistenceTransaction, DbConnection connection) = tuple;
            return new Domain.Transaction(
                this,
                persistenceTransaction.Id, 
                _flagFetcher(persistenceTransaction.FlagId, connection),
                persistenceTransaction.CheckNumber,
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