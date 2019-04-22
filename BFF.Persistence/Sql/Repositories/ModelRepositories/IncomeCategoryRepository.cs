using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    internal interface ISqliteIncomeCategoryRepositoryInternal : IIncomeCategoryRepository, ISqliteObservableRepositoryBaseInternal<IIncomeCategory>
    {
    }

    internal sealed class SqliteIncomeCategoryRepository : SqliteObservableRepositoryBase<IIncomeCategory, ICategorySql>, ISqliteIncomeCategoryRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ICategorySql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly Lazy<ICategoryOrm> _categoryOrm;

        private readonly TaskCompletionSource<Unit> _onConstructorCompleted = new TaskCompletionSource<Unit>();

        public SqliteIncomeCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategorySql> crudOrm,
            Lazy<IMergeOrm> mergeOrm,
            Lazy<ICategoryOrm> categoryOrm)
            : base( rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            _onConstructorCompleted.SetResult(Unit.Default);
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(ICategorySql persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new Models.Domain.IncomeCategory(
                    _crudOrm,
                    _mergeOrm.Value,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override async Task<IEnumerable<ICategorySql>> FindAllInnerAsync()
        {
            await _onConstructorCompleted.Task.ConfigureAwait(false);
            return await _categoryOrm.Value.ReadIncomeCategoriesAsync().ConfigureAwait(false);
        }
    }
}
