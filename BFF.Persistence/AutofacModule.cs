using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using BFF.Core.Persistence;
using BFF.Persistence.Contexts;
using BFF.Persistence.Realm.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using BFF.Persistence.Sql.Models;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.Repositories;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using BackendChoice = BFF.Core.IoC.BackendChoice;
using IImportContext = BFF.Core.IoC.IImportContext;
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

            builder.Register<Func<string, ILoadProjectFromFileConfiguration>>(cc =>
            {
                return path =>
                {
                    if (path.EndsWith(".sqlite") || path.EndsWith(".bffs"))
                    {
                        return new LoadProjectFromFileConfiguration(path);
                    }
                    throw new InvalidOperationException("Unknown extension");
                };
            });

            builder.Register<Func<ILoadProjectFromFileConfiguration, ScopeLevels, ILifetimeScope>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return (config, scopeLevel) =>
                {
                    var newLifetimeScope = currentLifetimeScope
                        .BeginLifetimeScope(
                            scopeLevel,
                            cb =>
                            {
                                switch (config.BackendChoice)
                                {
                                    case BackendChoice.Sqlite:
                                        LoadSqliteRegistrations(cb, config);
                                        break;
                                    case BackendChoice.Realm:
                                        LoadRealmRegistrations(cb, config);
                                        break;
                                    default: throw new InvalidEnumArgumentException(nameof(config), (int)config.BackendChoice, typeof(BackendChoice));
                                }
                            });

                    return newLifetimeScope;
                };
            });

            builder.Register<Func<string, ICreateProjectContext>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return path =>
                {
                    var config =
                        currentLifetimeScope.Resolve<ILoadProjectFromFileConfiguration>(TypedParameter.From(path));

                    var newLifetimeScope = currentLifetimeScope.Resolve<Func<ILoadProjectFromFileConfiguration, ScopeLevels, ILifetimeScope>>()(config, ScopeLevels.CreateProject);

                    return newLifetimeScope.Resolve<CreateProjectContext>(TypedParameter.From((IDisposable)newLifetimeScope));
                };
            });

            builder.Register<Func<(string, string, string, ImportFormatChoice), IImportContext>>(cc =>
            {
                var currentLifetimeScope = cc.Resolve<ILifetimeScope>();
                return t =>
                {
                    switch (t.Item4)
                    {
                        case ImportFormatChoice.Ynab4Csv:
                            var config = currentLifetimeScope.Resolve<IYnab4ImportConfiguration>(TypedParameter.From((t.Item1, t.Item2, t.Item3)));

                            var newLifetimeScope = currentLifetimeScope
                                .BeginLifetimeScope(
                                    ScopeLevels.Import);

                            return newLifetimeScope.Resolve<Ynab4ImportContext>(
                                TypedParameter.From((IDisposable) newLifetimeScope));
                        default: throw new InvalidEnumArgumentException(nameof(t), (int)t.Item4, typeof(ImportFormatChoice));
                    }
                };
            });

            builder.Register<Func<IImportingConfiguration, IImportContext>>(cc =>
            {
                return ic =>
                {
                    switch (ic)
                    {
                        case IYnab4ImportConfiguration ynab4:
                            return cc.Resolve<IYnab4ImportContext>(
                                TypedParameter.From(ynab4));
                        default:
                            throw new InvalidOperationException("Unknown import configuration");
                    }
                };
            });
        }

        private void LoadBackendRegistrationsCommon(ContainerBuilder builder, ILoadProjectFromFileConfiguration config)
        {
            builder.Register(cc => config)
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerMatchingLifetimeScope(
                    ScopeLevels.CreateProject,
                    ScopeLevels.LoadedProject);
        }

        private void LoadSqliteRegistrations(ContainerBuilder builder, ILoadProjectFromFileConfiguration config)
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
                .InstancePerMatchingLifetimeScope(
                    ScopeLevels.CreateProject,
                    ScopeLevels.LoadedProject);

            builder.RegisterGeneric(typeof(DapperCrudOrm<>))
                .AsSelf()
                .As(typeof(Sql.ORM.Interfaces.ICrudOrm<>))
                .InstancePerMatchingLifetimeScope(
                    ScopeLevels.CreateProject,
                    ScopeLevels.LoadedProject);
        }

        private void LoadRealmRegistrations(ContainerBuilder builder, ILoadProjectFromFileConfiguration config)
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
                typeof(RealmCreateBackendOrm))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerMatchingLifetimeScope(
                    ScopeLevels.CreateProject,
                    ScopeLevels.LoadedProject);

            builder.RegisterGeneric(typeof(RealmCrudOrm<>))
                .AsSelf()
                .As(typeof(Realm.ORM.Interfaces.ICrudOrm<>))
                .InstancePerMatchingLifetimeScope(
                    ScopeLevels.CreateProject,
                    ScopeLevels.LoadedProject);
        }
    }
}
