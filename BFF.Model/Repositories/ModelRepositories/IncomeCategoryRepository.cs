using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<IIncomeCategory>, IMergingRepository<IIncomeCategory>
    {
    }

    internal sealed class IncomeCategoryRepository : ObservableRepositoryBase<IIncomeCategory, ICategorySql>, IIncomeCategoryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;
        private readonly Func<ICategorySql> _categoryDtoFactory;

        public IncomeCategoryRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm,
            Func<ICategorySql> categoryDtoFactory)
            : base(provideConnection, rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            _categoryDtoFactory = categoryDtoFactory;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(ICategorySql persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory(this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<ICategorySql>> FindAllInnerAsync() => _categoryOrm.ReadIncomeCategoriesAsync();

        protected override Converter<IIncomeCategory, ICategorySql> ConvertToPersistence => domainCategory =>
        {
            var categoryDto = _categoryDtoFactory();

            categoryDto.Id = domainCategory.Id;
            categoryDto.Name = domainCategory.Name;
            categoryDto.ParentId = null;
            categoryDto.IsIncomeRelevant = true;
            categoryDto.MonthOffset = domainCategory.MonthOffset;

            return categoryDto;
        };

        public async Task MergeAsync(IIncomeCategory from, IIncomeCategory to)
        {
            await _mergeOrm.MergeCategoryAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
