using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using BFF.Model.IoC;
using System;

namespace BFF.Persistence.Common
{
    internal abstract class ContextFactoryBase<TConfiguration, TContext> : IContextFactory 
        where TConfiguration : IConfiguration
        where TContext : IContext
    {
        private readonly Func<TConfiguration, TContext> _factory;

        protected ContextFactoryBase(Func<TConfiguration, TContext> factory) => 
            _factory = factory;
        
        public bool ContextCanLoad => typeof(ILoadContext).IsAssignableFrom(typeof(TContext));
        
        public bool ContextCanImport => typeof(IImportContext).IsAssignableFrom(typeof(TContext));
        
        public bool ContextCanExport => typeof(IExportContext).IsAssignableFrom(typeof(TContext));
        public bool CanCreate(IConfiguration configuration) => configuration is TConfiguration;

        public IContext Create(IConfiguration configuration) =>
            configuration is TConfiguration realmConfiguration 
                ? _factory(realmConfiguration) 
                : throw new Exception();
    }
}