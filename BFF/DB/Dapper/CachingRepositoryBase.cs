using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Helper.Extensions;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using NLog;
using Domain = BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public interface ICachingRepositoryBase<TDomain> : IRepositoryBase<TDomain> where TDomain : class, Domain.IDataModel
    {
    }

    public abstract class CachingRepositoryBase<TDomain, TPersistence> 
        : RepositoryBase<TDomain, TPersistence>, ICachingRepositoryBase<TDomain>
        where TDomain : class, Domain.IDataModel 
        where TPersistence : class, IPersistenceModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly Dictionary<long, TDomain> _cache = new Dictionary<long, TDomain>();

        protected CachingRepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(
            provideConnection, crudOrm)
        {
            Disposable.Create(ClearCache).AddTo(CompositeDisposable);
        }

        public override async Task AddAsync(TDomain dataModel)
        {
            await base.AddAsync(dataModel).ConfigureAwait(false);
            if(!_cache.ContainsKey(dataModel.Id))
                _cache.Add(dataModel.Id, dataModel);
        }

        public override async Task<TDomain> FindAsync(long id)
        {
            if(!_cache.ContainsKey(id))
                _cache.Add(id, await base.FindAsync(id).ConfigureAwait(false));
            return _cache[id];
        }

        public override async Task DeleteAsync(TDomain dataModel)
        {
            await base.DeleteAsync(dataModel).ConfigureAwait(false);
            RemoveFromCache(dataModel);
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
            if (_cache.ContainsKey(dataModel.Id))
                _cache.Remove(dataModel.Id);
        }

        protected void ClearCache()
        {
            _cache.Clear();
        }
    }
}