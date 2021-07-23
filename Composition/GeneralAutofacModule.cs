using Autofac;
using BFF.Core.IoC;
using BFF.ViewModel.ViewModels.ForModels;
using System.Diagnostics;
using System.Reflection;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public class GeneralAutofacModule : Module
    {
        private readonly Assembly[] _assemblies;

        public GeneralAutofacModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(_assemblies)
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

            builder.RegisterAssemblyTypes(_assemblies)
                .Where(t =>
                {
                    var isAssignable = typeof(IOncePerApplication).IsAssignableFrom(t);

                    Debug.WriteLineIf(isAssignable, $"Once Per Application - {t.Name}");

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(_assemblies)
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

            builder.RegisterAssemblyTypes(_assemblies)
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
        }
    }
}