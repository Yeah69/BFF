using Autofac;
using BFF.Core.IoC;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using StrongInject;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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
            var stringBuilder = new StringBuilder();

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
                .Where(t =>
                {
                    if (t.IsInterface.Not() && (t.FullName?.Contains('+').Not() ?? false) && t.GenericTypeArguments.None())
                    {
                        Type[] interfaces = t.GetInterfaces()
                            .Where(i => (i.FullName?.StartsWith("BFF.") ?? true)
                                && (i.FullName?.Contains('+').Not() ?? false)
                                && i.GenericTypeArguments.None())
                            .ToArray();
                        var interfacesPart = interfaces.Any()
                            ? $", {string.Join(", ", interfaces.Select(i => $"typeof({i.FullName})"))}"
                            : "";
                        stringBuilder = stringBuilder.AppendLine($"[Register(typeof({t.FullName}){interfacesPart})]");
                    }
                    
                    return true;
                })
                .AsImplementedInterfaces()
                .AsSelf();

            builder.Build();

            var text = stringBuilder.ToString();

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