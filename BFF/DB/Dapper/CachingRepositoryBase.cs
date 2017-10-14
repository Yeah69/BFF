using System.Collections.Generic;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using Dapper.Contrib.Extensions;
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

        protected CachingRepositoryBase(IProvideConnection provideConnection) : base(provideConnection) { }

        public override void Add(TDomain dataModel, DbConnection connection = null)
        {
            base.Add(dataModel, connection);
            if(!_cache.ContainsKey(dataModel.Id))
                _cache.Add(dataModel.Id, dataModel);
        }

        public override TDomain Find(long id, DbConnection connection = null)
        {
            if(!_cache.ContainsKey(id))
            {
                _cache.Add(id, base.Find(id, connection));
            }
            return _cache[id];
        }

        public override void Delete(TDomain dataModel, DbConnection connection = null)
        {
            base.Delete(dataModel, connection);
            if(!_cache.ContainsKey(dataModel.Id))
                _cache.Remove(dataModel.Id);
        }

        public override IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            IEnumerable<TPersistence> elements = ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.GetAll<TPersistence>(), 
                ProvideConnection, 
                connection);
            Logger.Debug("Starting to convert all POCOs of type {0}", typeof(TPersistence).Name);
            foreach(TPersistence element in elements)
            {
                if(!_cache.ContainsKey(element.Id))
                    _cache.Add(element.Id, ConvertToDomain( (element, connection) ));
                yield return _cache[element.Id];
            }
            Logger.Debug("Finished converting all POCOs of type {0}", typeof(TPersistence).Name);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _cache.Clear();
            }
        }
    }
}