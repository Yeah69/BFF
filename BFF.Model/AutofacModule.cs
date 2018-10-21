using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using BFF.Core;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence;
using BFF.Persistence.Contexts;
using BFF.Persistence.Import;
using Module = Autofac.Module;

namespace BFF.Model
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
                .AsImplementedInterfaces();

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

            builder.Register<Func<ITransaction>>(cc =>
            {
                var repository = cc.Resolve<IRepository<ITransaction>>();
                var rxSchedulerProvider = cc.Resolve<IRxSchedulerProvider>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Transaction(
                    repository,
                    rxSchedulerProvider,
                    lastSetDate.Date);
            }).As<Func<ITransaction>>();

            builder.Register<Func<ITransfer>>(cc =>
            {
                var repository = cc.Resolve<IRepository<ITransfer>>();
                var rxSchedulerProvider = cc.Resolve<IRxSchedulerProvider>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Transfer(
                    repository,
                    rxSchedulerProvider,
                    lastSetDate.Date);
            }).As<Func<ITransfer>>();

            builder.Register<Func<IParentTransaction>>(cc =>
            {
                var repository = cc.Resolve<IRepository<IParentTransaction>>();
                var rxSchedulerProvider = cc.Resolve<IRxSchedulerProvider>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new ParentTransaction(
                    repository,
                    rxSchedulerProvider,
                    Enumerable.Empty<ISubTransaction>(),
                    lastSetDate.Date);
            }).As<Func<IParentTransaction>>();

            builder.Register<Func<IAccount>>(cc =>
            {
                var repository = cc.Resolve<IRepository<IAccount>>();
                var rxSchedulerProvider = cc.Resolve<IRxSchedulerProvider>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Account(
                    repository,
                    rxSchedulerProvider,
                    lastSetDate.Date);
            }).As<Func<IAccount>>();

            builder.Register<Func<ISubTransaction>>(cc =>
            {
                var repository = cc.Resolve<IRepository<ISubTransaction>>();
                var rxSchedulerProvider = cc.Resolve<IRxSchedulerProvider>();
                return () => new SubTransaction(
                    repository,
                    rxSchedulerProvider);
            }).As<Func<ISubTransaction>>();

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

            builder.RegisterModule(new Persistence.AutofacModule());
        }
    }
}
