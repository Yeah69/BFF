using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateIncomeTable : CreateTableBase
    {
        public CreateIncomeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Income)}s](
            {nameof(Income.Id)} INTEGER PRIMARY KEY,
            {nameof(Income.AccountId)} INTEGER,
            {nameof(Income.PayeeId)} INTEGER,
            {nameof(Income.CategoryId)} INTEGER,
            {nameof(Income.Date)} DATE,
            {nameof(Income.Memo)} TEXT,
            {nameof(Income.Sum)} INTEGER,
            {nameof(Income.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Income.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Income.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL,
            FOREIGN KEY({nameof(Income.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

    }

    public interface IIncomeRepository : IRepositoryBase<Domain.IIncome>
    {
    }

    public class IncomeRepository : RepositoryBase<Domain.IIncome, Income>, IIncomeRepository
    {
        private readonly Func<long, DbConnection, Domain.IAccount> _accountFetcher;
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;
        private readonly Func<long, DbConnection, Domain.IPayee> _payeeFetcher;

        public IncomeRepository(
            IProvideConnection provideConnection, 
            Func<long, DbConnection, Domain.IAccount> accountFetcher,
            Func<long, DbConnection, Domain.ICategory> categoryFetcher,
            Func<long, DbConnection, Domain.IPayee> payeeFetcher) : base(provideConnection)
        {
            _accountFetcher = accountFetcher;
            _categoryFetcher = categoryFetcher;
            _payeeFetcher = payeeFetcher;
        }

        public override Domain.IIncome Create() =>
            new Domain.Income(this, -1L, DateTime.MinValue, null, null, null, "", 0L, false);
        
        protected override Converter<Domain.IIncome, Income> ConvertToPersistence => domainIncome => 
            new Income
            {
                Id = domainIncome.Id,
                AccountId = domainIncome.Account.Id,
                CategoryId = domainIncome.Category.Id,
                PayeeId = domainIncome.Payee.Id,
                Date = domainIncome.Date,
                Memo = domainIncome.Memo,
                Sum = domainIncome.Sum,
                Cleared = domainIncome.Cleared ? 1L : 0L
            };

        protected override Converter<(Income, DbConnection), Domain.IIncome> ConvertToDomain => tuple =>
        {
            (Income persistenceIncome, DbConnection connection) = tuple;
            return new Domain.Income(
                this,
                persistenceIncome.Id,
                persistenceIncome.Date,
                _accountFetcher(persistenceIncome.AccountId, connection),
                _payeeFetcher(persistenceIncome.PayeeId, connection),
                _categoryFetcher(persistenceIncome.CategoryId, connection),
                persistenceIncome.Memo,
                persistenceIncome.Sum,
                persistenceIncome.Cleared == 1L);
        };
    }
}