using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<IIncomeCategory>
    {
    }

    internal interface IIncomeCategoryRepositoryInternal : IIncomeCategoryRepository, IMergingRepository<IIncomeCategory>, IReadOnlyRepository<IIncomeCategory>
    {
    }

    internal sealed class IncomeCategoryRepository : ObservableRepositoryBase<IIncomeCategory, ICategorySql>, IIncomeCategoryRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;

        public IncomeCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategorySql> crudOrm,
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm)
            : base( rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(ICategorySql persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory<ICategorySql>(
                    persistenceModel,
                    this,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Id > 0,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<ICategorySql>> FindAllInnerAsync() => _categoryOrm.ReadIncomeCategoriesAsync();
        
        public async Task MergeAsync(IIncomeCategory from, IIncomeCategory to)
        {
            var fromPersistenceModel = (@from as IDataModelInternal<ICategorySql>)?.BackingPersistenceModel;
            var toPersistenceModel = (to as IDataModelInternal<ICategorySql>)?.BackingPersistenceModel;
            if (fromPersistenceModel is null || toPersistenceModel is null) return;
            await _mergeOrm.MergeCategoryAsync(
                fromPersistenceModel,
                toPersistenceModel).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
