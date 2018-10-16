﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<IIncomeCategory>, IMergingRepository<IIncomeCategory>
    {
    }

    public sealed class IncomeCategoryRepository : ObservableRepositoryBase<IIncomeCategory, CategoryDto>, IIncomeCategoryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;

        public IncomeCategoryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm)
            : base(provideConnection, rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(CategoryDto persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory(this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<CategoryDto>> FindAllInnerAsync() => _categoryOrm.ReadIncomeCategoriesAsync();

        protected override Converter<IIncomeCategory, CategoryDto> ConvertToPersistence => domainCategory =>
            new CategoryDto
            {
                Id = domainCategory.Id,
                Name = domainCategory.Name,
                ParentId = null,
                IsIncomeRelevant = true,
                MonthOffset = domainCategory.MonthOffset
            };

        public async Task MergeAsync(IIncomeCategory from, IIncomeCategory to)
        {
            await _mergeOrm.MergeCategoryAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
