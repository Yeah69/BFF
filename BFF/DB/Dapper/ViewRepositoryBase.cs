using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.Dapper
{
    public abstract class ViewRepositoryBase<TDomainBase, TPersistance, TSpecifying>
        : IViewRepository<TDomainBase, TSpecifying>
        where TDomainBase : class, IDataModel
        where TPersistance : class, IPersistanceModel
    {
        private IProvideConnection _provideConnection;
        
        
        
        protected ViewRepositoryBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }
        
        public virtual IEnumerable<TDomainBase> GetPage(int offset, int pageSize, TSpecifying specifyingObject, 
                                                        DbConnection connection = null)
        {
            string query = $"SELECT * FROM [{ViewName}] {GetSpecifyingPageSuffix(specifyingObject)} {GetOrderingPageSuffix(specifyingObject)} LIMIT {offset}, {pageSize};";
            
            return ConnectionHelper.QueryOnExistingOrNewConnectionAndReturn(
                c => c.Query<TPersistance>(query).Select(tt => ConvertToDomain(tt)), 
                _provideConnection, 
                connection);
        }

        public virtual int GetCount(TSpecifying specifyingObject, DbConnection connection = null)
        {
            string query = $"SELECT COUNT(*) FROM [{ViewName}] {GetSpecifyingCountSuffix(specifyingObject)};";
            
            return ConnectionHelper.QueryOnExistingOrNewConnectionAndReturn(
                c => c.Query<int>(query), 
                _provideConnection,
                connection).First();
        }
        
        protected abstract Converter<TPersistance, TDomainBase> ConvertToDomain { get; }
        protected abstract string ViewName { get; }
        protected abstract string GetOrderingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingPageSuffix(TSpecifying specifyingObject);
        protected abstract string GetSpecifyingCountSuffix(TSpecifying specifyingObject);
    }
}