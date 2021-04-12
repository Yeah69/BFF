using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using System;
using System.Collections.Generic;
using Module = Autofac.Module;

namespace BFF.Model
{
    public interface ILifetimeScopeRegistry
    {
        void Add(object key, ILifetimeScope lifetimeScope);

        ILifetimeScope Get(object key);
    }

    internal class LifetimeScopeRegistry : ILifetimeScopeRegistry, IDisposable
    {
        private readonly IDictionary<object, ILifetimeScope> _registry = new Dictionary<object, ILifetimeScope>();
        
        public void Add(object key, ILifetimeScope lifetimeScope) => _registry[key] = lifetimeScope;
        public ILifetimeScope Get(object key) => _registry[key];

        public void Dispose() => _registry.Clear();
    }
    
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly()
            };

            builder.RegisterAssemblyTypes(assemblies)
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
                        Debug.WriteLine("Transient - " + t.Name);
                    }

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .ExternallyOwned();
        }
    }
}
