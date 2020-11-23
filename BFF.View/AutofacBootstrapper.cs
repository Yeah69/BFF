using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.View.Helper;
using MahApps.Metro.Controls.Dialogs;
using MainWindow = BFF.View.Views.MainWindow;

namespace BFF.View
{
    public static class AutofacBootstrapper
    {
        private static ILifetimeScope? _rootScope;

        static AutofacBootstrapper()
        {
            Start();
        }

        private static void Start()
        {
            if (_rootScope is not null)
            {
                return;
            }

            var builder = new ContainerBuilder();
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

            builder.Register(cc => DialogCoordinator.Instance).As<IDialogCoordinator>();

            builder.RegisterType<WpfRxSchedulerProvider>().As<IRxSchedulerProvider>().SingleInstance();

            builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

            builder.RegisterModule(new Core.AutoFacModule());

            builder.RegisterModule(new ViewModel.AutofacModule());

            builder.RegisterModule(new Persistence.Proxy.AutofacModule());

            _rootScope = builder.Build();
        }

        public static void Stop()
        {
            _rootScope?.Dispose();
        }

        public static T Resolve<T>() where T : notnull
        {
            if (_rootScope is null)
                throw new Exception("Bootstrapper hasn't been started!");

            return _rootScope.Resolve<T>();
        }
    }
}
