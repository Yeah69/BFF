using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class SubIncomeRepository : RepositoryBase<Domain.SubIncome, Persistance.SubIncome>
    {
        public SubIncomeRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.SubIncome)}s](
            {nameof(Persistance.SubIncome.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.SubIncome.ParentId)} INTEGER,
            {nameof(Persistance.SubIncome.CategoryId)} INTEGER,
            {nameof(Persistance.SubIncome.Memo)} TEXT,
            {nameof(Persistance.SubIncome.Sum)} INTEGER,
            FOREIGN KEY({nameof(Persistance.SubIncome.ParentId)}) REFERENCES {nameof(Persistance.ParentIncome)}s({nameof(Persistance.ParentIncome.Id)}) ON DELETE CASCADE);";
        
        protected override Converter<Domain.SubIncome, Persistance.SubIncome> ConvertToPersistance => domainSubIncome => 
            new Persistance.SubIncome
            {
                Id = domainSubIncome.Id,
                ParentId = domainSubIncome.ParentId,
                CategoryId = domainSubIncome.CategoryId,
                Memo = domainSubIncome.Memo,
                Sum = domainSubIncome.Sum
            };
        
        protected override Converter<Persistance.SubIncome, Domain.SubIncome> ConvertToDomain => persistanceSubIncome =>
            new Domain.SubIncome
            {
                Id = persistanceSubIncome.Id,
                ParentId = persistanceSubIncome.ParentId,
                CategoryId = persistanceSubIncome.CategoryId,
                Memo = persistanceSubIncome.Memo,
                Sum = persistanceSubIncome.Sum
            };
    }
}