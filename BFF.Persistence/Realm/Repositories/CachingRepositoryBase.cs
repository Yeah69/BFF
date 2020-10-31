using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Domain;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Models.Persistence;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using NLog;

namespace BFF.Persistence.Realm.Repositories
{
    internal interface IRealmCachingRepositoryBase<TDomain, TPersistence> : IRealmRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        void RemoveFromCache(TDomain dataModel);
        void ClearCache();
    }

    internal abstract class RealmCachingRepositoryBase<TDomain, TPersistence>
        : RealmRepositoryBase<TDomain, TPersistence>, IRealmCachingRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        private readonly IRealmOperations _realmOperations;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<TDomain> _cache = new HashSet<TDomain>();

        protected RealmCachingRepositoryBase(
            ICrudOrm<TPersistence> crudOrm,
            IRealmOperations realmOperations) : base(crudOrm)
        {
            _realmOperations = realmOperations;
            Disposable.Create(ClearCache).CompositeDisposalWith(CompositeDisposable);
        }

        public override async Task<bool> AddAsync(TDomain dataModel)
        {
            if (!(dataModel is IRealmModel<TPersistence>)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            var result = await base.AddAsync(dataModel).ConfigureAwait(false);
            if (!_cache.Contains(dataModel))
                _cache.Add(dataModel);
            return result;
        }

        public override async Task<TDomain> FindAsync(TPersistence realmObject)
        {
            var dataModel = await _realmOperations
                .RunFuncAsync(_ =>
                    _cache.FirstOrDefault(dm => ((IRealmModel<TPersistence>) dm).RealmObject?.Equals(realmObject) ?? false))
                .ConfigureAwait(false);
            if (dataModel is null)
            {
                dataModel = await base.FindAsync(realmObject).ConfigureAwait(false);
                _cache.Add(dataModel);
            }
            return dataModel;
        }


        protected override async Task<IEnumerable<TDomain>> FindAllAsync()
        {
            Logger.Debug("Starting to convert all POCOs of type {0}", typeof(TPersistence).Name);
            ICollection<TDomain> ret = new List<TDomain>();
            foreach (TPersistence element in await FindAllInnerAsync().ConfigureAwait(false))
            {
                var dataModel = await FindAsync(element).ConfigureAwait(false);
                dataModel.AddTo(ret);
            }
            Logger.Debug("Finished converting all POCOs of type {0}", typeof(TPersistence).Name);
            return ret;
        }

        public void RemoveFromCache(TDomain dataModel)
        {
            if (!(dataModel is IRealmModel<TPersistence>)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            if (_cache.Contains(dataModel))
                _cache.Remove(dataModel);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}