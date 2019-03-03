using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using Module = Autofac.Module;

namespace BFF.ViewModel
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
                        Debug.WriteLine("Transient view model - " + t.Name);
                    }

                    return isAssignable;
                })
                .AsImplementedInterfaces()
                .ExternallyOwned();

            builder.RegisterType<MainWindowViewModel>().As<IMainWindowViewModel>().SingleInstance();

            builder.Register<Func<IAccountBaseViewModel, ITransactionViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel>>();
                var createNewModels = cc.Resolve<ICreateNewModels>();
                return abvm => factory(createNewModels.CreateTransaction(), abvm);
            }).As<Func<IAccountBaseViewModel, ITransactionViewModel>>();

            builder.Register<Func<IAccountBaseViewModel, ITransferViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<ITransfer, IAccountBaseViewModel, ITransferViewModel>>();
                var createNewModels = cc.Resolve<ICreateNewModels>();
                return abvm => factory(createNewModels.CreateTransfer(), abvm);
            }).As<Func<IAccountBaseViewModel, ITransferViewModel>>();

            builder.Register<Func<IAccountBaseViewModel, IParentTransactionViewModel>>(cc =>
            {
                var factory = cc.Resolve<Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel>>();
                var createNewModels = cc.Resolve<ICreateNewModels>();
                return abvm => factory(createNewModels.CreateParentTransfer(), abvm);
            }).As<Func<IAccountBaseViewModel, IParentTransactionViewModel>>();

            builder.RegisterModule(new Model.AutofacModule());
        }
    }
}
