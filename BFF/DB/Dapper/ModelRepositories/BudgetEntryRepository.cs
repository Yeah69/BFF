using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateBudgetEntryTable : CreateTableBase
    {
        public CreateBudgetEntryTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(BudgetEntry)}s](
            {nameof(BudgetEntry.Id)} INTEGER PRIMARY KEY,
            {nameof(BudgetEntry.CategoryId)} INTEGER,
            {nameof(BudgetEntry.Month)} DATE,
            {nameof(BudgetEntry.Budget)} INTEGER,
            FOREIGN KEY({nameof(BudgetEntry.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
    }

    public interface IBudgetEntryRepository : IRepositoryBase<Domain.IBudgetEntry>
    {
    }


    public class BudgetEntryRepository : RepositoryBase<Domain.IBudgetEntry, BudgetEntry>, IBudgetEntryRepository
    {
        private readonly Func<long, DbConnection, Domain.ICategory> _categoryFetcher;
        public BudgetEntryRepository(IProvideConnection provideConnection, Func<long, DbConnection, Domain.ICategory> categoryFetcher) : base(provideConnection)
        {
            _categoryFetcher = categoryFetcher;
        }

        public override Domain.IBudgetEntry Create() =>
            new Domain.BudgetEntry(this, -1L, DateTime.MinValue);
        
        protected override Converter<Domain.IBudgetEntry, BudgetEntry> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        protected override Converter<(BudgetEntry, DbConnection), Domain.IBudgetEntry> ConvertToDomain => tuple =>
        {
            (BudgetEntry persistenceBudgetEntry, DbConnection connection) = tuple;
            return new Domain.BudgetEntry(
                this,
                persistenceBudgetEntry.Id,
                persistenceBudgetEntry.Month,
                _categoryFetcher(persistenceBudgetEntry.CategoryId, connection),
                persistenceBudgetEntry.Budget);
        };
    }
}