﻿using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;
using BFF.Persistence.Import;
using BFF.Persistence.Realm;
using BFF.Persistence.Realm.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using BFF.Persistence.Sql.Models;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.Repositories;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using Module = Autofac.Module;

namespace BFF.Persistence
{
    public class AutoFacModule : Module
    {
        private static readonly object[] LifetimeScopeTagsForOrmRegistrations = 
        {
            ScopeLevels.CreateProject,
            ScopeLevels.LoadedProject,
            ScopeLevels.Import,
            ScopeLevels.Export
        };

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

            builder.Register<Func<string, IFileAccessConfiguration>>(cc =>
            {
                return path =>
                {
                    if (path.EndsWith(".sqlite") || path.EndsWith(".bffs"))
                    {
                        return new SqliteFileAccessConfiguration(path);
                    }
                    throw new InvalidOperationException("Unknown extension");
                };
            });

            builder.Register<Func<IFileAccessConfiguration, ScopeLevels, ILifetimeScope>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return (config, scopeLevel) =>
                {
                    var newLifetimeScope = currentLifetimeScope
                        .BeginLifetimeScope(
                            scopeLevel,
                            cb =>
                            {
                                switch (config)
                                {
                                    case ISqliteFileAccessConfiguration sqliteConfiguration:
                                        LoadSqliteRegistrations(cb, sqliteConfiguration);
                                        break;
                                    case IRealmFileAccessConfiguration realmConfiguration:
                                        LoadRealmRegistrations(cb, realmConfiguration);
                                        break;
                                    default: throw new ArgumentException("Unsupported configuration", nameof(config));
                                }
                            });

                    return newLifetimeScope;
                };
            });

            builder.Register<Func<IFileAccessConfiguration, ICreateProjectContext>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return config =>
                {
                    var newLifetimeScope = currentLifetimeScope.Resolve<Func<IFileAccessConfiguration, ScopeLevels, ILifetimeScope>>()(config, ScopeLevels.CreateProject);

                    return newLifetimeScope.Resolve<CreateProjectContext>(TypedParameter.From((IDisposable)newLifetimeScope));
                };
            });

            builder.Register<Func<IImportingConfiguration, IImporter>>(cc =>
            {
                var ynab4CsvImporterFactory = cc.Resolve<Func<IYnab4CsvImportConfiguration, Ynab4CsvImporter>>();
                return ic =>
                {
                    switch (ic)
                    {
                        case IYnab4CsvImportConfiguration ynab4:
                            return ynab4CsvImporterFactory(ynab4);
                        default:
                            throw new InvalidOperationException("Unknown import configuration");
                    }
                };
            });

            builder.Register<Func<IExportingConfiguration, IExporter>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return ec =>
                {
                    switch (ec)
                    {
                        case IRealmExportConfiguration realmExportConfiguration:
                            var loadProjectFromFileConfiguration = 
                                new RealmFileAccessConfiguration(
                                    realmExportConfiguration.Path,
                                    realmExportConfiguration.Password);
                            var newLifetimeScope = currentLifetimeScope
                                .BeginLifetimeScope(
                                    ScopeLevels.Export,
                                    cb => LoadRealmRegistrations(cb, loadProjectFromFileConfiguration));

                            return newLifetimeScope.Resolve<RealmExporter>(TypedParameter.From<IDisposable>(newLifetimeScope));
                        default: throw new ArgumentException(nameof(ec));
                    }
                };
            });
        }

        private void LoadBackendRegistrationsCommon<T>(
            ContainerBuilder builder, 
            T config) where T : ILoadProjectConfiguration 
        {
            builder.Register(cc => config)
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerMatchingLifetimeScope(LifetimeScopeTagsForOrmRegistrations);
        }

        private void LoadSqliteRegistrations(ContainerBuilder builder, ISqliteFileAccessConfiguration config)
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
                .InstancePerMatchingLifetimeScope(LifetimeScopeTagsForOrmRegistrations);

            builder.RegisterGeneric(typeof(DapperCrudOrm<>))
                .AsSelf()
                .As(typeof(Sql.ORM.Interfaces.ICrudOrm<>))
                .InstancePerMatchingLifetimeScope(LifetimeScopeTagsForOrmRegistrations);
        }

        private void LoadRealmRegistrations(ContainerBuilder builder, IRealmFileAccessConfiguration config)
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
                typeof(RealmOperations))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerMatchingLifetimeScope(LifetimeScopeTagsForOrmRegistrations);

            builder.RegisterGeneric(typeof(RealmCrudOrm<>))
                .AsSelf()
                .As(typeof(Realm.ORM.Interfaces.ICrudOrm<>))
                .InstancePerMatchingLifetimeScope(LifetimeScopeTagsForOrmRegistrations);
        }
    }
}
