using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using BFF.Model;
using BFF.Model.Contexts;
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
                .Where(t =>
                {
                    if (t.Name.StartsWith("BFFPersistence_ProcessedByFody") || t.Name.StartsWith("RealmModuleInitializer"))
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

            builder.Register<Func<IImportConfiguration, IImportContext>>(cc =>
            {
                var ynab4CsvImporterFactory = cc.Resolve<Func<IYnab4CsvImportConfiguration, Ynab4CsvImporter>>();
                var sqliteContextFactory = cc.Resolve<Func<ISqliteProjectFileAccessConfiguration, SqliteContext>>();
                return ic =>
                {
                    switch (ic)
                    {
                        case IYnab4CsvImportConfiguration ynab4:
                            return ynab4CsvImporterFactory(ynab4);
                        case ISqliteProjectFileAccessConfiguration sqlite:
                            return sqliteContextFactory(sqlite);
                        default:
                            throw new InvalidOperationException("Unknown import configuration");
                    }
                };
            });

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
                    typeof(Sql.Models.Domain.SummaryAccount),
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
                .As(typeof(Sql.ORM.Interfaces.ICrudOrm<>))
                .InstancePerLifetimeScope();
        }

        private void LoadRealmRegistrations(ContainerBuilder builder, IRealmProjectFileAccessConfiguration config)
        {
            LoadBackendRegistrationsCommon(builder, config);

            builder.RegisterTypes(
                typeof(Realm.Models.Domain.SummaryAccount),
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
                .As(typeof(Realm.ORM.Interfaces.ICrudOrm<>))
                .InstancePerLifetimeScope();
        }
    }
}
