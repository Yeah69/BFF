using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.Dapper
{
    public abstract class ViewRepositoryBase<TDomainBase, TPersistence, TSpecifying>
        : IViewRepository<TDomainBase, TSpecifying>, IViewRepositoryAsync<TDomainBase, TSpecifying>
        where TDomainBase : class, IDataModel
        where TPersistence : class, IPersistanceModel
    {
        private readonly IProvideConnection _provideConnection;
        
        
        
        protected ViewRepositoryBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }
        
        public virtual IEnumerable<TDomainBase> GetPage(int offset, int pageSize, TSpecifying specifyingObject, 
                                                        DbConnection connection = null)
        {
            string query = $"SELECT * FROM [{ViewName}] {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix(specifyingObject)} LIMIT {offset}, {pageSize};";

            return ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<TPersistence>(query).Select(tt => ConvertToDomain((tt, c))),
                _provideConnection,
                connection);
        }

        public virtual int GetCount(TSpecifying specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM [{ViewName}] {GetSpecifyingCountSuffix(specifyingObject)};";
            
            return ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<int>(query), 
                _provideConnection,
                connection).First();
        }

        public virtual async Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject,
            DbConnection connection = null)
        {
            string query = $"SELECT * FROM [{ViewName}] {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix(specifyingObject)} LIMIT {offset}, {pageSize};";

            return await ConnectionHelper.QueryOnExistingOrNewConnectionAsync(
                async c => (await c.QueryAsync<TPersistence>(query)).Select(tt => ConvertToDomain((tt, c))),
                _provideConnection,
                connection);
        }

        public virtual async Task<int> GetCountAsync(TSpecifying specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM [{ViewName}] {GetSpecifyingCountSuffix(specifyingObject)};";

            return (await ConnectionHelper.QueryOnExistingOrNewConnectionAsync(
                async c => await c.QueryAsync<int>(query),
                _provideConnection,
                connection)).First();
        }

        protected abstract Converter<(TPersistence, DbConnection), TDomainBase> ConvertToDomain { get; }
        protected abstract string ViewName { get; }
        protected abstract string GetOrderingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingCountSuffix(TSpecifying specifyingObject);
    }
}