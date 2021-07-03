using Autofac;
using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public interface ILifetimeScopeRegistry
    {
        void Add(object key, ILifetimeScope lifetimeScope);

        ILifetimeScope Get(object key);
    }

    internal class LifetimeScopeRegistry : ILifetimeScopeRegistry, IOncePerApplication
    {
        private readonly IDictionary<object, ILifetimeScope> _registry = new Dictionary<object, ILifetimeScope>();

        public LifetimeScopeRegistry(CompositeDisposable compositeDisposable) =>
            Disposable
                .Create(_registry, r => r.Clear())
                .CompositeDisposalWith(compositeDisposable);

        public void Add(object key, ILifetimeScope lifetimeScope) => _registry[key] = lifetimeScope;
        public ILifetimeScope Get(object key) => _registry[key];
    }

    public class ViewModelAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterInstance(BudgetEntryViewModelPlaceholder.Instance)
                .As<BudgetEntryViewModelPlaceholder>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<MainWindowViewModel>()
                .As<IMainWindowViewModel>()
                .SingleInstance();

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

            builder.Register<Func<IContext, ILoadContextViewModel>>(cc =>
            {
                var lifetimeScopeRegistry = cc.Resolve<ILifetimeScopeRegistry>();
                return context => lifetimeScopeRegistry
                    .Get(context)
                    .Resolve<ILoadContextViewModel>(
                        TypedParameter.From(context));
            });

            builder.Register<Func<IEmptyContextViewModel>>(cc =>
            {
                var lifetimeScope = cc.Resolve<ILifetimeScope>();
                return () =>
                {
                    var scope = lifetimeScope.BeginLifetimeScope();
                    return scope.Resolve<IEmptyContextViewModel>(TypedParameter.From((IDisposable)scope));
                };
            });

            builder.RegisterType<BudgetEntryViewModel>().AsImplementedInterfaces();

            builder.RegisterType<LifetimeScopeRegistry>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<CurrentTextsViewModel>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}