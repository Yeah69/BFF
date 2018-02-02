using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;

namespace BFF.DB.Dapper
{
    public abstract class CreateTableBase : ICreateTable
    {
        private readonly IProvideConnection _provideConnection;
        
        protected CreateTableBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public virtual void CreateTable(DbConnection connection = null)
        {
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                c => c.Execute(CreateTableStatement), 
                _provideConnection,
                connection);
        }
        
        protected abstract string CreateTableStatement { get; }
    }

    public interface IRepositoryBase<TDomain> : IWriteOnlyRepositoryBase<TDomain>, IDbTableRepository<TDomain> where TDomain : class, IDataModel
    {
    }

    public abstract class RepositoryBase<TDomain, TPersistence> : WriteOnlyRepositoryBase<TDomain, TPersistence>, IRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        private readonly ICrudOrm _crudOrm;

        protected RepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(provideConnection, crudOrm)
        {
            _crudOrm = crudOrm;
        }

        public virtual TDomain Find(long id, DbConnection connection = null)
        {
            return ConvertToDomain((_crudOrm.Read<TPersistence>(id), connection));
        }

        protected abstract Converter<(TPersistence, DbConnection), TDomain> ConvertToDomain { get; }

        protected virtual IEnumerable<TPersistence> FindAllInner(DbConnection connection) => _crudOrm.ReadAll<TPersistence>();

        public virtual IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            return FindAllInner(connection).Select(p => ConvertToDomain((p, connection)));
        }
    }
}