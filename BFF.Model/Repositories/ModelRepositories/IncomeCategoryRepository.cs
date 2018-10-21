using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

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

    internal sealed class IncomeCategoryRepository : ObservableRepositoryBase<IIncomeCategory, ICategoryDto>, IIncomeCategoryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;
        private readonly Func<ICategoryDto> _categoryDtoFactory;

        public IncomeCategoryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm,
            Func<ICategoryDto> categoryDtoFactory)
            : base(provideConnection, rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            _categoryDtoFactory = categoryDtoFactory;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(ICategoryDto persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory(this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<ICategoryDto>> FindAllInnerAsync() => _categoryOrm.ReadIncomeCategoriesAsync();

        protected override Converter<IIncomeCategory, ICategoryDto> ConvertToPersistence => domainCategory =>
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
