using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using NLog;

namespace BFF.Model.Repositories
{
    internal interface ICachingRepositoryBase<TDomain, TPersistence> : IRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
    }

    internal abstract class CachingRepositoryBase<TDomain, TPersistence> 
        : RepositoryBase<TDomain, TPersistence>, ICachingRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel 
        where TPersistence : class, IPersistenceModelSql
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Dictionary<long, TDomain> _cache = new Dictionary<long, TDomain>();

        protected CachingRepositoryBase(ICrudOrm<TPersistence> crudOrm) : base(crudOrm)
        {
            Disposable.Create(ClearCache).AddTo(CompositeDisposable);
        }

        public override async Task<bool> AddAsync(TDomain dataModel)
        {
            var persistenceModel = (dataModel as IDataModelInternal<TPersistence>).BackingPersistenceModel;
            var result = await base.AddAsync(dataModel).ConfigureAwait(false);
            if(!_cache.ContainsKey(persistenceModel.Id))
                _cache.Add(persistenceModel.Id, dataModel);
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

        protected void RemoveFromCache(TDomain dataModel)
        {
            var persistenceModel = (dataModel as IDataModelInternal<TPersistence>).BackingPersistenceModel;
            if (_cache.ContainsKey(persistenceModel.Id))
                _cache.Remove(persistenceModel.Id);
        }

        protected void ClearCache()
        {
            _cache.Clear();
        }
    }
}