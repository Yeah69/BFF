using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
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

        public virtual TDomain Find(long id)
        {
            return ConvertToDomain(_crudOrm.Read<TPersistence>(id));
        }

        protected abstract Converter<TPersistence, TDomain> ConvertToDomain { get; }

        protected virtual IEnumerable<TPersistence> FindAllInner() => _crudOrm.ReadAll<TPersistence>();

        public virtual IEnumerable<TDomain> FindAll()
        {
            return FindAllInner().Select(p => ConvertToDomain(p));
        }
    }
}