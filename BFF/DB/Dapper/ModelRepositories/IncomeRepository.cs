using System;
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
        public IncomeRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.Income, Persistance.Income> ConvertToPersistance => domainIncome => 
            new Persistance.Income
            {
                Id = domainIncome.Id,
                AccountId = domainIncome.AccountId,
                CategoryId = domainIncome.CategoryId,
                PayeeId = domainIncome.PayeeId,
                Date = domainIncome.Date,
                Memo = domainIncome.Memo,
                Sum = domainIncome.Sum,
                Cleared = domainIncome.Cleared ? 1L : 0L
            };
        
        protected override Converter<Persistance.Income, Domain.Income> ConvertToDomain => persistanceIncome =>
            new Domain.Income(persistanceIncome.Date)
            {
                Id = persistanceIncome.Id,
                AccountId = persistanceIncome.AccountId,
                CategoryId = persistanceIncome.CategoryId,
                PayeeId = persistanceIncome.PayeeId,
                Date = persistanceIncome.Date,
                Memo = persistanceIncome.Memo,
                Sum = persistanceIncome.Sum,
                Cleared = persistanceIncome.Cleared == 1L
            };
    }
}