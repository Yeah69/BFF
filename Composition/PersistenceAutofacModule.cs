using Autofac;
using BFF.Model.ImportExport;
using BFF.Persistence.Import;
using BFF.Persistence.Realm;
using BFF.Persistence.Realm.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using BFF.Persistence.Sql;
using BFF.Persistence.Sql.Models;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.Repositories;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
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

            builder.Register<Func<ISqliteProjectFileAccessConfiguration, ISqliteContext>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                var lifetimeScopeRegistry = cc.Resolve<ILifetimeScopeRegistry>();
                return config =>
                {
                    var newLifetimeScope = currentLifetimeScope
                        .BeginLifetimeScope(cb => LoadSqliteRegistrations(cb, config));
                    var context = newLifetimeScope.Resolve<ISqliteContext>(TypedParameter.From<IDisposable>(newLifetimeScope));
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

        private void LoadSqliteRegistrations(ContainerBuilder builder, ISqliteProjectFileAccessConfiguration config)
        {
            LoadBackendRegistrationsCommon(builder, config);

            builder.RegisterTypes(
                    typeof(Persistence.Sql.Models.Domain.SummaryAccount),
                    typeof(SqliteCreateNewModels),
                    typeof(SqliteAccountRepository),
                    typeof(SqliteBudgetEntryRepository),
                    typeof(SqliteCategoryBaseRepository),
                    typeof(SqliteCategoryRepository),
                    typeof(SqliteDbSettingRepository),
                    typeof(SqliteFlagRepository),
                    typeof(SqliteIncomeCategoryRepository),
                    typeof(SqliteParentTransactionRepository),
                    typeof(SqlitePayeeRepository),
                    typeof(SqliteSubTransactionRepository),
                    typeof(SqliteTransactionRepository),
                    typeof(SqliteTransferRepository),
                    typeof(SqliteTransRepository),
                    typeof(SqliteBudgetMonthRepository),
                    typeof(DapperCreateBackendOrm))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(DapperCrudOrm<>))
                .AsSelf()
                .As(typeof(Persistence.Sql.ORM.Interfaces.ICrudOrm<>))
                .InstancePerLifetimeScope();
        }

        private void LoadRealmRegistrations(ContainerBuilder builder, IRealmProjectFileAccessConfiguration config)
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