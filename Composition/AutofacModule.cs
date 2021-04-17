using Autofac;
using BFF.Core.IoC;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reflection;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public interface ILifetimeScopeRegistry
    {
        void Add(object key, ILifetimeScope lifetimeScope);

        ILifetimeScope Get(object key);
    }

    internal class LifetimeScopeRegistry : ILifetimeScopeRegistry, IOncePerApplication
    {
        private readonly IDictionary<object, ILifetimeScope> _registry = new Dictionary<object, ILifetimeScope>();

        public LifetimeScopeRegistry(CompositeDisposable compositeDisposable) =>
            Disposable
                .Create(_registry, r => r.Clear())
                .CompositeDisposalWith(compositeDisposable);

        public void Add(object key, ILifetimeScope lifetimeScope) => _registry[key] = lifetimeScope;
        public ILifetimeScope Get(object key) => _registry[key];
    }
    
    public class AutofacModule : Module
    {
        public static (View.Wpf.App, IDisposable) ResolveWpfApp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            var container = builder.Build();
            var app = container.BeginLifetimeScope().Resolve<View.Wpf.App>();
            return (app, container);
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var assemblies = new[]
            {
                Assembly.Load(typeof(View.Wpf.AssemblyInfo).Assembly.GetName()),
                //Assembly.Load(typeof(View.Avalonia.AssemblyInfo).Assembly.GetName()), // todo
                Assembly.Load(typeof(ViewModel.AssemblyInfo).Assembly.GetName()),
                Assembly.Load(typeof(Model.AssemblyInfo).Assembly.GetName()),
                Assembly.Load(typeof(Persistence.AssemblyInfo).Assembly.GetName()),
                Assembly.Load(typeof(Core.AssemblyInfo).Assembly.GetName())
            };

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.Namespace?.StartsWith("BFF") ?? false)
                .Where(t => t != typeof(BudgetEntryViewModelPlaceholder))
                .Where(t =>
                {
                    if (t.Name.StartsWith("Persistence_ProcessedByFody") 
                        || t.Name.StartsWith("BFFPersistence_ProcessedByFody") 
                        || t.Name.StartsWith("RealmModuleInitializer"))
                    {
                        return false;
                    }

                    return true;
                })
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t =>
                {
                    var isAssignable = typeof(IOncePerApplication).IsAssignableFrom(t);

                    Debug.WriteLineIf(isAssignable, $"Once Per Application - {t.Name}");

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t =>
                {
                    var isAssignable = typeof(IOncePerBackend).IsAssignableFrom(t);

                    Debug.WriteLineIf(isAssignable, $"Once Per LifetimeScope - {t.Name}");

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            // several view model instances are transitory and created on the fly, if these are tracked by the container then they
            // won't be disposed of in a timely manner

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t =>
                {
                    var isAssignable = typeof(ITransient).IsAssignableFrom(t);
                    if (isAssignable)
                    {
                        Debug.WriteLine("Transient view model - " + t.Name);
                    }

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.RegisterModule(new PersistenceAutofacModule());

            builder.RegisterModule(new ViewModelAutofacModule());

            builder.RegisterModule(new ViewAutofacModule());

            builder.Register(_ => new CompositeDisposable())
                .InstancePerLifetimeScope();

            builder.RegisterType<LifetimeScopeRegistry>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}