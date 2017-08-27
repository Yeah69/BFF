using System;
using System.Data.Common;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateBudgetEntryTable : CreateTableBase
    {
        public CreateBudgetEntryTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.BudgetEntry)}s](
            {nameof(Persistance.BudgetEntry.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.BudgetEntry.CategoryId)} INTEGER,
            {nameof(Persistance.BudgetEntry.Month)} DATE,
            {nameof(Persistance.BudgetEntry.Budget)} INTEGER,
            FOREIGN KEY({nameof(Persistance.BudgetEntry.CategoryId)}) REFERENCES {nameof(Persistance.Category)}s({nameof(Persistance.Category.Id)}) ON DELETE SET NULL);";
    }
    
    public class BudgetEntryRepository : RepositoryBase<Domain.IBudgetEntry, Persistance.BudgetEntry>
    {
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;
        public BudgetEntryRepository(IProvideConnection provideConnection, Func<long, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.IBudgetEntry Create() =>
            new Domain.BudgetEntry(this, -1L, DateTime.MinValue);
        
        protected override Converter<Domain.IBudgetEntry, Persistance.BudgetEntry> ConvertToPersistance => domainBudgetEntry => 
            new Persistance.BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        protected override Converter<(Persistance.BudgetEntry, DbConnection), Domain.IBudgetEntry> ConvertToDomain => tuple =>
        {
            (Persistance.BudgetEntry persistenceBudgetEntry, DbConnection connection) = tuple;
            return new Domain.BudgetEntry(
                this,
                persistenceBudgetEntry.Id,
                persistenceBudgetEntry.Month,
                _categoryFetcher(persistenceBudgetEntry.CategoryId, connection),
                persistenceBudgetEntry.Budget);
        };
    }
}