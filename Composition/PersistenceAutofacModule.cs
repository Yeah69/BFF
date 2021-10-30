using Autofac;
using BFF.Model.ImportExport;
using BFF.Persistence.Import;
using BFF.Persistence.Realm;
using BFF.Persistence.Realm.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using System;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public class PersistenceAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register<Func<IRealmProjectFileAccessConfiguration, IRealmContext>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                var lifetimeScopeRegistry = cc.Resolve<ILifetimeScopeRegistry>();
                return config =>
                {
                    var newLifetimeScope = currentLifetimeScope
                        .BeginLifetimeScope(cb => LoadRealmRegistrations(cb, config));
                    var context = newLifetimeScope.Resolve<IRealmContext>(TypedParameter.From<IDisposable>(newLifetimeScope));
                    lifetimeScopeRegistry.Add(context, newLifetimeScope);
                    return context;
                };
            });

            builder.Register<Func<IYnab4CsvImportConfiguration, IYnab4CsvImporter>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                var lifetimeScopeRegistry = cc.Resolve<ILifetimeScopeRegistry>();
                return config =>
                {
                    var newLifetimeScope = currentLifetimeScope.BeginLifetimeScope(
                        b => LoadBackendRegistrationsCommon(b, config));
                    var context = newLifetimeScope.Resolve<IYnab4CsvImporter>(TypedParameter.From<IDisposable>(newLifetimeScope));
                    lifetimeScopeRegistry.Add(context, newLifetimeScope);
                    return context;
                };
            });

            builder.RegisterType<DateTimeStaticDelegate>().AsImplementedInterfaces();
        }

        private static void LoadBackendRegistrationsCommon<T>(
            ContainerBuilder builder, 
            T config) where T : IConfiguration 
        {
            builder.Register(_ => config)
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

        private static void LoadRealmRegistrations(ContainerBuilder builder, IRealmProjectFileAccessConfiguration config)
        {
            LoadBackendRegistrationsCommon(builder, config);

            builder.RegisterTypes(
                typeof(Persistence.Realm.Models.Domain.SummaryAccount),
                typeof(RealmCreateNewModels),
                typeof(RealmAccountRepository),
                typeof(RealmBudgetEntryRepository),
                typeof(RealmCategoryBaseRepository),
                typeof(RealmCategoryRepository),
                typeof(RealmDbSettingRepository),
                typeof(RealmFlagRepository),
                typeof(RealmIncomeCategoryRepository),
                typeof(RealmParentTransactionRepository),
                typeof(RealmPayeeRepository),
                typeof(RealmSubTransactionRepository),
                typeof(RealmTransactionRepository),
                typeof(RealmTransferRepository),
                typeof(RealmTransRepository),
                typeof(RealmBudgetMonthRepository),
                typeof(RealmCreateBackendOrm),
                typeof(RealmOperations),
                typeof(RealmExporter))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(RealmCrudOrm<>))
                .AsSelf()
                .As(typeof(Persistence.Realm.ORM.Interfaces.ICrudOrm<>))
                .InstancePerLifetimeScope();
        }
    }
}