using System;
using BFF.Core.Persistence;
using BFF.Model.Contexts;

namespace BFF.Persistence.Contexts
{
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
