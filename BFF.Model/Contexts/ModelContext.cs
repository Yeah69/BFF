using System;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;

namespace BFF.Model.Contexts
{
    public interface IModelContext { }

    internal class ModelContext : IModelContext
    {
        public ModelContext(
            IPersistenceConfiguration persistenceConfiguration,
            Func<IPersistenceConfiguration, IPersistenceContext> persistenceContextFactory)
        {
            persistenceContextFactory(persistenceConfiguration);
        }
    }
}
