using System;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class BudgetEntryRepository : RepositoryBase<Domain.BudgetEntry, Persistance.BudgetEntry>
    {
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.BudgetEntry)}s](
            {nameof(Persistance.BudgetEntry.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.BudgetEntry.CategoryId)} INTEGER,
            {nameof(Persistance.BudgetEntry.Month)} DATE,
            {nameof(Persistance.BudgetEntry.Budget)} INTEGER,
            FOREIGN KEY({nameof(Persistance.BudgetEntry.CategoryId)}) REFERENCES {nameof(Persistance.Category)}s({nameof(Persistance.Category.Id)}) ON DELETE SET NULL);";
        
        public BudgetEntryRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        protected override Converter<Domain.BudgetEntry, Persistance.BudgetEntry> ConvertToPersistance => domainBudgetEntry => 
            new Persistance.BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.CategoryId,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };
        
        protected override Converter<Persistance.BudgetEntry, Domain.BudgetEntry> ConvertToDomain => persistanceBudgetEntry =>
            new Domain.BudgetEntry(persistanceBudgetEntry.Month)
            {
                Id = persistanceBudgetEntry.Id,
                CategoryId = persistanceBudgetEntry.CategoryId,
                Month = persistanceBudgetEntry.Month,
                Budget = persistanceBudgetEntry.Budget
            };
    }
}