using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.MVVM.Views;
using MahApps.Metro.Controls.Dialogs;
using Transaction = BFF.MVVM.Models.Native.Transaction;

namespace BFF
{
    public static class AutoFacBootstrapper
    {
        private static ILifetimeScope _rootScope;

        static AutoFacBootstrapper()
        {
            Start();
        }

        private static void Start()
        {
            if (_rootScope != null)
            {
                return;
            }

            var builder = new ContainerBuilder();
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

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
                    var isAssignable = typeof(ITransientViewModel).IsAssignableFrom(t);
                    if (isAssignable)
                    {
                        Debug.WriteLine("Transient view model - " + t.Name);
                    }

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();

            builder.Register<Func<ITransaction>>(cc =>
            {
                return () => new Transaction(
                    cc.Resolve<IRepository<ITransaction>>(), 
                    cc.Resolve<IRxSchedulerProvider>(),
                    cc.Resolve<INotifyBudgetOverviewRelevantChange>(),
                    cc.Resolve<ILastSetDate>().Date);
            }).As<Func<ITransaction>>();

            builder.Register<Func<IAccountBaseViewModel, ITransactionViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel>>();
                return abvm => factory(cc.Resolve<Func<ITransaction>>()(), abvm);
            }).As<Func<IAccountBaseViewModel, ITransactionViewModel>>();

            builder.Register<Func<ITransfer>>(cc =>
            {
                return () => new Transfer(
                    cc.Resolve<IRepository<ITransfer>>(),
                    cc.Resolve<IRxSchedulerProvider>(),
                    cc.Resolve<INotifyBudgetOverviewRelevantChange>(),
                    cc.Resolve<ILastSetDate>().Date);
            }).As<Func<ITransfer>>();

            builder.Register<Func<IAccountBaseViewModel, ITransferViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<ITransfer, IAccountBaseViewModel, ITransferViewModel>>();
                return abvm => factory(cc.Resolve<Func<ITransfer>>()(), abvm);
            }).As<Func<IAccountBaseViewModel, ITransferViewModel>>();

            builder.Register<Func<IParentTransaction>>(cc =>
            {
                return () => new ParentTransaction(
                    cc.Resolve<IRepository<IParentTransaction>>(),
                    cc.Resolve<IRxSchedulerProvider>(),
                    cc.Resolve<INotifyBudgetOverviewRelevantChange>(),
                    Enumerable.Empty<ISubTransaction>(), 
                    cc.Resolve<ILastSetDate>().Date);
            }).As<Func<IParentTransaction>>();

            builder.Register<Func<IAccountBaseViewModel, IParentTransactionViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel>>();
                return abvm => factory(cc.Resolve<Func<IParentTransaction>>()(), abvm);
            }).As<Func<IAccountBaseViewModel, IParentTransactionViewModel>>();

            builder.Register<Func<IAccount>>(cc =>
            {
                return () => new Account(
                    cc.Resolve<IRepository<IAccount>>(), 
                    cc.Resolve<IRxSchedulerProvider>(),
                    cc.Resolve<INotifyBudgetOverviewRelevantChange>(),
                    cc.Resolve<ILastSetDate>().Date);
            }).As<Func<IAccount>>();

            builder.Register(cc => DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<WpfRxSchedulerProvider>().As<IRxSchedulerProvider>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

            _rootScope = builder.Build();
        }

        public static void Stop()
        {
            _rootScope.Dispose();
        }

        public static T Resolve<T>()
        {
            if (_rootScope is null)
            {
                throw new Exception("Bootstrapper hasn't been started!");
            }

            return _rootScope.Resolve<T>();
        }
    }
}
