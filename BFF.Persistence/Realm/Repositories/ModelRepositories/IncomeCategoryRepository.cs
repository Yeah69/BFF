using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    internal interface IRealmIncomeCategoryRepositoryInternal : IIncomeCategoryRepository, IRealmObservableRepositoryBaseInternal<IIncomeCategory, ICategoryRealm>
    {
    }

    internal sealed class RealmIncomeCategoryRepository : RealmObservableRepositoryBase<IIncomeCategory, ICategoryRealm>, IRealmIncomeCategoryRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ICategoryRealm> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly Lazy<ICategoryOrm> _categoryOrm;

        private readonly TaskCompletionSource<Unit> _onConstructorCompleted = new TaskCompletionSource<Unit>();

        public RealmIncomeCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategoryRealm> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IMergeOrm> mergeOrm,
            Lazy<ICategoryOrm> categoryOrm)
            : base( rxSchedulerProvider, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            _onConstructorCompleted.SetResult(Unit.Default);
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(ICategoryRealm persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IIncomeCategory InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.IncomeCategory(
                    _crudOrm,
                    _mergeOrm.Value,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset);
            }
        }

        protected override async Task<IEnumerable<ICategoryRealm>> FindAllInnerAsync()
        {
            await _onConstructorCompleted.Task.ConfigureAwait(false);
            return await _categoryOrm.Value.ReadIncomeCategoriesAsync().ConfigureAwait(false);
        }
    }
}
