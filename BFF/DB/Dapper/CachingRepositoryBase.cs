using System.Collections.Generic;
using System.Data.Common;
using Dapper.Contrib.Extensions;
using NLog;
using Persistence = BFF.DB.PersistanceModels;
using Domain = BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public abstract class CachingRepositoryBase<TDomain, TPersistence> 
        : RepositoryBase<TDomain, TPersistence> 
        where TDomain : class, Domain.IDataModel 
        where TPersistence : class, Persistence.IPersistanceModel
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
            IEnumerable<TPersistence> pocos = ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.GetAll<TPersistence>(), 
                ProvideConnection, 
                connection);
            Logger.Debug("Starting to convert all POCOs of type {0}", typeof(TPersistence).Name);
            foreach(TPersistence poco in pocos)
            {
                if(!_cache.ContainsKey(poco.Id))
                    _cache.Add(poco.Id, ConvertToDomain( (poco, connection) ));
                yield return _cache[poco.Id];
            }
            Logger.Debug("Finished converting all POCOs of type {0}", typeof(TPersistence).Name);
        }
    }
}