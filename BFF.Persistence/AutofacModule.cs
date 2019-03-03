using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.ORM.Interfaces;
using Module = Autofac.Module;

namespace BFF.Persistence
{
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
                        Debug.WriteLine("Transient view model - " + t.Name);
                    }

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.Register<Func<IPersistenceConfiguration, IPersistenceContext>>(cc =>
            {
                return pc =>
                {
                    switch (pc)
                    {
                        case ISqlitePersistenceConfiguration sqlite:
                            return cc.Resolve<ISqlitePersistenceContext>(TypedParameter.From(sqlite));
                        default:
                            throw new InvalidOperationException("Unknown persistence configuration");
                    }
                };
            });

            builder.Register<Func<IImportingConfiguration, IPersistenceConfiguration, IImportContext>>(cc =>
            {
                return (ic, pc) =>
                {
                    switch (ic)
                    {
                        case IYnab4ImportConfiguration ynab4:
                            return cc.Resolve<IYnab4ImportContext>(
                                TypedParameter.From(ynab4),
                                TypedParameter.From(pc));
                        default:
                            throw new InvalidOperationException("Unknown import configuration");
                    }
                };
            });

            builder.RegisterGeneric(typeof(DapperCrudOrm<>))
                .AsSelf()
                .As(typeof(ICrudOrm<>))
                .InstancePerLifetimeScope();
        }
    }
}
