using Autofac;
using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.ViewModel.Contexts;
using MrMeeseeks.Reactive.Extensions;
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

    internal class LifetimeScopeRegistry : ILifetimeScopeRegistry, IContainerInstance
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

            builder.RegisterType<LifetimeScopeRegistry>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}