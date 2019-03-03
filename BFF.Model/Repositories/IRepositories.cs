using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using MuVaViMo;

namespace BFF.Model.Repositories
{
    public interface IObservableRepositoryBase<TDomain> : IOncePerBackend 
        where TDomain : class, IDataModel
    {
        IObservableReadOnlyList<TDomain> All { get; }
        Task<IDeferredObservableReadOnlyList<TDomain>> AllAsync { get; }
        IObservable<IEnumerable<TDomain>> ObserveResetAll { get; }
    }
}
