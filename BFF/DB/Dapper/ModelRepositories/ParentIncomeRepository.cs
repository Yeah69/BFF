using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateParentIncomeTable : CreateTableBase
    {
        public CreateParentIncomeTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.ParentIncome)}s](
            {nameof(Persistance.ParentIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.ParentIncome.AccountId)} INTEGER,
            {nameof(Persistance.ParentIncome.PayeeId)} INTEGER,
            {nameof(Persistance.ParentIncome.Date)} DATE,
            {nameof(Persistance.ParentIncome.Memo)} TEXT,
            {nameof(Persistance.ParentIncome.Cleared)} INTEGER,
            FOREIGN KEY({nameof(Persistance.ParentIncome.AccountId)}) REFERENCES {nameof(Persistance.Account)}s({nameof(Persistance.Account.Id)}) ON DELETE CASCADE,
            FOREIGN KEY({nameof(Persistance.ParentIncome.PayeeId)}) REFERENCES {nameof(Persistance.Payee)}s({nameof(Persistance.Payee.Id)}) ON DELETE SET NULL);";
        
    }
    
    public class ParentIncomeRepository : RepositoryBase<Domain.ParentIncome, Persistance.ParentIncome>
    {
        public ParentIncomeRepository(IProvideConnection provideConnection) : base(provideConnection) { }

        public override Domain.ParentIncome Create() =>
            new Domain.ParentIncome(this, DateTime.MinValue);
        
        protected override Converter<Domain.ParentIncome, Persistance.ParentIncome> ConvertToPersistance => domainParentIncome => 
            new Persistance.ParentIncome
            {
                Id = domainParentIncome.Id,
                AccountId = domainParentIncome.AccountId,
                PayeeId = domainParentIncome.PayeeId,
                Date = domainParentIncome.Date,
                Memo = domainParentIncome.Memo,
                Cleared = domainParentIncome.Cleared ? 1L : 0L
            };
        
        protected override Converter<Persistance.ParentIncome, Domain.ParentIncome> ConvertToDomain => persistanceParentIncome =>
            new Domain.ParentIncome(this, persistanceParentIncome.Date)
            {
                Id = persistanceParentIncome.Id,
                AccountId = persistanceParentIncome.AccountId,
                PayeeId = persistanceParentIncome.PayeeId,
                Date = persistanceParentIncome.Date,
                Memo = persistanceParentIncome.Memo,
                Cleared = persistanceParentIncome.Cleared == 1L
            };
    }
}