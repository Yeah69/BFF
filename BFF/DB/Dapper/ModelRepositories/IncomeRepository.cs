using System;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateIncomeTable : CreateTableBase
    {
        public CreateIncomeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Income)}s](
            {nameof(Persistance.Income.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Income.AccountId)} INTEGER,
            {nameof(Persistance.Income.PayeeId)} INTEGER,
            {nameof(Persistance.Income.CategoryId)} INTEGER,
            {nameof(Persistance.Income.Date)} DATE,
            {nameof(Persistance.Income.Memo)} TEXT,
            {nameof(Persistance.Income.Sum)} INTEGER,
            {nameof(Persistance.Income.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.Income.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.Income.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL,
            FOREIGN KEY({nameof(Persistance.Income.CategoryId)}) REFERENCES {nameof(Persistance.Category)}s({nameof(Persistance.Category.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class IncomeRepository : RepositoryBase<Domain.Income, Persistance.Income>
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

        public override Domain.Income Create() =>
            new Domain.Income(this, -1L, DateTime.MinValue, null, null, null, "", 0L, false);
        
        protected override Converter<Domain.Income, Persistance.Income> ConvertToPersistance => domainIncome => 
            new Persistance.Income
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

        protected override Converter<(Persistance.Income, DbConnection), Domain.Income> ConvertToDomain => tuple =>
        {
            (Persistance.Income persistenceIncome, DbConnection connection) = tuple;
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