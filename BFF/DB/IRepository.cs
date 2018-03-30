﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IWriteOnlyRepository<in T> : IOncePerBackend where T : class, IDataModel
    {
        Task AddAsync(T dataModel);
        Task UpdateAsync(T dataModel);
        Task DeleteAsync(T dataModel);
    }
    public interface IReadOnlyRepository<T> : IOncePerBackend where T : class, IDataModel
    {
        Task<T> FindAsync(long id);
    }

    public interface IRepository<T> : 
        IReadOnlyRepository<T>, IWriteOnlyRepository<T>
        where T : class, IDataModel
    {
    }

    public interface ISpecifiedPagedAccessAsync<TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject);
        Task<long> GetCountAsync(TSpecifying specifyingObject);
    }

    public interface ICollectiveRepository<T> : IOncePerBackend where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    public interface IDbTableRepository<T> : IRepository<T>, ICollectiveRepository<T>
        where T : class, IDataModel { }
}