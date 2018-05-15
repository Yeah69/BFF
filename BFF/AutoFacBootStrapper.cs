using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels;
using BFF.MVVM.ViewModels.ForModels;
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
                var repository = cc.Resolve<IRepository<ITransaction>>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Transaction(repository, lastSetDate.Date);
            }).As<Func<ITransaction>>();

            builder.Register<Func<ITransactionViewModel>>(cc =>
            {
                var modelFactory = cc.Resolve<Func<ITransaction>>();
                var factory = cc.Resolve<Func<ITransaction, ITransactionViewModel>>();
                return () => factory(modelFactory());
            }).As<Func<ITransactionViewModel>>();

            builder.Register<Func<IAccount, ITransactionViewModel>>(cc =>
            {
                var modelFactory = cc.Resolve<Func<ITransaction>>();
                var factory = cc.Resolve<Func<ITransaction, ITransactionViewModel>>();
                return account =>
                {
                    var model = modelFactory();
                    model.Account = account;
                    return factory(model);
                };
            }).As<Func<IAccount, ITransactionViewModel>>();

            builder.Register<Func<ITransfer>>(cc =>
            {
                var repository = cc.Resolve<IRepository<ITransfer>>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Transfer(repository, lastSetDate.Date);
            }).As<Func<ITransfer>>();

            builder.Register<Func<ITransferViewModel>>(cc =>
            {
                var modelFactory = cc.Resolve<Func<ITransfer>>();
                var factory = cc.Resolve<Func<ITransfer, ITransferViewModel>>();
                return () => factory(modelFactory());
            }).As<Func<ITransferViewModel>>();

            builder.Register<Func<IParentTransaction>>(cc =>
            {
                var repository = cc.Resolve<IRepository<IParentTransaction>>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new ParentTransaction(repository, Enumerable.Empty<ISubTransaction>(), lastSetDate.Date);
            }).As<Func<IParentTransaction>>();

            builder.Register<Func<IAccount, IParentTransactionViewModel>>(cc =>
            {
                var modelFactory = cc.Resolve<Func<IParentTransaction>>();
                var factory = cc.Resolve<Func<IParentTransaction, IParentTransactionViewModel>>();
                return account =>
                {
                    var model = modelFactory();
                    model.Account = account;
                    return factory(model);
                };
            }).As<Func<IAccount, IParentTransactionViewModel>>();

            builder.Register<Func<IAccount>>(cc =>
            {
                var repository = cc.Resolve<IRepository<IAccount>>();
                var lastSetDate = cc.Resolve<ILastSetDate>();
                return () => new Account(repository, lastSetDate.Date);
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
