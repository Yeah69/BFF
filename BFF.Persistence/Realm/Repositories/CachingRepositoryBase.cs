using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Domain;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Models.Persistence;
using MrMeeseeks.Extensions;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<TPersistence, TDomain> _cache = new Dictionary<TPersistence, TDomain>();

        protected RealmCachingRepositoryBase(ICrudOrm<TPersistence> crudOrm) : base(crudOrm)
        {
            Disposable.Create(ClearCache).AddForDisposalTo(CompositeDisposable);
        }

        public override async Task<bool> AddAsync(TDomain dataModel)
        {
            if (!(dataModel is IRealmModel<TPersistence>)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            var realmObject = ((IRealmModel<TPersistence>)dataModel).RealmObject;
            var result = await base.AddAsync(dataModel).ConfigureAwait(false);
            if (!_cache.ContainsKey(realmObject))
                _cache.Add(realmObject, dataModel);
            return result;
        }

        public override async Task<TDomain> FindAsync(TPersistence realmObject)
        {
            if (!_cache.ContainsKey(realmObject))
                _cache.Add(realmObject, await base.FindAsync(realmObject).ConfigureAwait(false));
            return _cache[realmObject];
        }



        public override async Task<IEnumerable<TDomain>> FindAllAsync()
        {
            Logger.Debug("Starting to convert all POCOs of type {0}", typeof(TPersistence).Name);
            ICollection<TDomain> ret = new List<TDomain>();
            foreach (TPersistence element in await FindAllInnerAsync().ConfigureAwait(false))
            {
                if (!_cache.ContainsKey(element))
                    _cache.Add(element, await ConvertToDomainAsync(element).ConfigureAwait(false));
                _cache[element].AddTo(ret);
            }
            Logger.Debug("Finished converting all POCOs of type {0}", typeof(TPersistence).Name);
            return ret;
        }

        public void RemoveFromCache(TDomain dataModel)
        {
            if (!(dataModel is IRealmModel<TPersistence>)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            var realmObject = ((IRealmModel<TPersistence>)dataModel).RealmObject;
            if (_cache.ContainsKey(realmObject))
                _cache.Remove(realmObject);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}