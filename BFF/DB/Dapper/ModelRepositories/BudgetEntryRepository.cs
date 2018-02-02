using System;
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

    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<Domain.IBudgetEntry>
    {
        Domain.IBudgetEntry Convert(BudgetEntry budgetEntry, long outflow, long balance);
    }


    public sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<Domain.IBudgetEntry, BudgetEntry>, IBudgetEntryRepository
    {
        private readonly ICategoryRepository _categoryRepository;

        public BudgetEntryRepository(IProvideConnection provideConnection, ICrudOrm crudOrm, ICategoryRepository categoryRepository ) : base(provideConnection, crudOrm)
        {
            _categoryRepository = categoryRepository;
        }
        
        protected override Converter<Domain.IBudgetEntry, BudgetEntry> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category?.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        public Domain.IBudgetEntry Convert(BudgetEntry budgetEntry, long outflow, long balance)
        {
            return new Domain.BudgetEntry(
                this, 
                budgetEntry.Id, 
                budgetEntry.Month,
                _categoryRepository.Find(budgetEntry.CategoryId ?? 0L), 
                budgetEntry.Budget, 
                outflow, 
                balance);
        }
    }
}