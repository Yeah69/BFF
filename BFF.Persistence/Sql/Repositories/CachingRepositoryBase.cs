using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Domain;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MrMeeseeks.Extensions;
using NLog;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface ISqliteCachingRepositoryBase<TDomain> : ISqliteRepositoryBase<TDomain>
        where TDomain : class, IDataModel
    {
        void RemoveFromCache(TDomain dataModel);
        void ClearCache();
    }

    internal abstract class SqliteCachingRepositoryBase<TDomain, TPersistence> 
        : SqliteRepositoryBase<TDomain, TPersistence>, ISqliteCachingRepositoryBase<TDomain>
        where TDomain : class, IDataModel 
        where TPersistence : class, IPersistenceModelSql
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Dictionary<long, TDomain> _cache = new Dictionary<long, TDomain>();

        protected SqliteCachingRepositoryBase(ICrudOrm<TPersistence> crudOrm) : base(crudOrm)
        {
            Disposable.Create(ClearCache).AddTo(CompositeDisposable);
        }

        public override async Task<bool> AddAsync(TDomain dataModel)
        {
            if (!(dataModel is ISqlModel)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            var id = ((ISqlModel)dataModel).Id;
            var result = await base.AddAsync(dataModel).ConfigureAwait(false);
            if(!_cache.ContainsKey(id))
                _cache.Add(id, dataModel);
            return result;
        }

        public override async Task<TDomain> FindAsync(long id)
        {
            if(!_cache.ContainsKey(id))
                _cache.Add(id, await base.FindAsync(id).ConfigureAwait(false));
            return _cache[id];
        }

        

        public override async Task<IEnumerable<TDomain>> FindAllAsync()
        {
            Logger.Debug("Starting to convert all POCOs of type {0}", typeof(TPersistence).Name);
            ICollection<TDomain> ret = new List<TDomain>();
            foreach(TPersistence element in await FindAllInnerAsync().ConfigureAwait(false))
            {
                if(!_cache.ContainsKey(element.Id))
                    _cache.Add(element.Id, await ConvertToDomainAsync(element).ConfigureAwait(false));
                _cache[element.Id].AddTo(ret);
            }
            Logger.Debug("Finished converting all POCOs of type {0}", typeof(TPersistence).Name);
            return ret;
        }

        public void RemoveFromCache(TDomain dataModel)
        {
            if (!(dataModel is ISqlModel)) throw new ArgumentException("Model instance has not the correct type", nameof(dataModel));

            var id = ((ISqlModel) dataModel).Id;
            if (_cache.ContainsKey(id))
                _cache.Remove(id);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}