using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface ICategoryBase : ICommonProperty
    {
    }

    internal abstract class CategoryBase<TDomain, TPersistence> : CommonProperty<TDomain, TPersistence>, ICategoryBase 
        where TDomain : class, ICategoryBase
        where TPersistence : class, IPersistenceModel
    {
        protected CategoryBase(
            TPersistence backingPersistenceModel,
            IRepository<TDomain, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            bool isInserted, 
            string name) : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, name)
        {
        }
    }
}
