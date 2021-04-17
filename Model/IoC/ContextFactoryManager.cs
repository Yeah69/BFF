using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using MrMeeseeks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BFF.Model.IoC
{
    public interface IContextManager
    {
        IContext Empty { get; }
        bool CanCreateLoadContext(IConfiguration configuration);
        bool CanCreateImportContext(IConfiguration configuration);
        bool CanCreateExportContext(IConfiguration configuration);
        ILoadContext CreateLoadContext(IConfiguration configuration);
        IImportContext CreateImportContext(IConfiguration configuration);
        IExportContext CreateExportContext(IConfiguration configuration);
    }
    
    internal class ContextManager : IContextManager, IOncePerApplication
    {
        private readonly IReadOnlyList<IContextFactory> _factories;

        public ContextManager(
            IReadOnlyList<IContextFactory> factories,
            EmptyContext emptyContext)
        {
            Empty = emptyContext;
            _factories = factories;

            var loads = _factories.Where(ContextCanLoad).ToReadOnlyList();
            var imports = _factories.Where(ContextCanImport).ToReadOnlyList();
            var exports = _factories.Where(ContextCanExport).ToReadOnlyList();
        }

        public IContext Empty { get; }

        public bool CanCreateLoadContext(IConfiguration configuration) => 
            CanCreateInner(configuration, ContextCanLoad);

        public bool CanCreateImportContext(IConfiguration configuration) => 
            CanCreateInner(configuration, ContextCanImport);

        public bool CanCreateExportContext(IConfiguration configuration) => 
            CanCreateInner(configuration, ContextCanExport);

        public ILoadContext CreateLoadContext(IConfiguration configuration) => 
            this.CreateInner<ILoadContext>(configuration, ContextCanLoad);

        public IImportContext CreateImportContext(IConfiguration configuration) => 
            this.CreateInner<IImportContext>(configuration, ContextCanImport);

        public IExportContext CreateExportContext(IConfiguration configuration) => 
            this.CreateInner<IExportContext>(configuration, ContextCanExport);

        private bool CanCreateInner(IConfiguration configuration, Func<IContextFactory, bool> specificPredicate) =>
            _factories.Any(f => f.CanCreate(configuration) && specificPredicate(f));
        
        private T CreateInner<T>(IConfiguration configuration, Func<IContextFactory, bool> specificPredicate) =>
            _factories
                .FirstOrDefault(f => f.CanCreate(configuration) && specificPredicate(f)) is { } factory
                ? (T)factory.Create(configuration)
                : throw new Exception();

        private static bool ContextCanLoad(IContextFactory contextFactory) => contextFactory.ContextCanLoad;
        private static bool ContextCanImport(IContextFactory contextFactory) => contextFactory.ContextCanImport;
        private static bool ContextCanExport(IContextFactory contextFactory) => contextFactory.ContextCanExport;
    }
}