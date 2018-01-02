using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.MVVM.ViewModels;

namespace BFF
{
    public static class AutoFacBootStrapper
    {
        private static ILifetimeScope _rootScope;
        private static IMainWindowViewModel _mainWindowViewModel;

        public static IMainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (_rootScope == null)
                {
                    Start();
                }

                _mainWindowViewModel = _rootScope.Resolve<IMainWindowViewModel>();
                return _mainWindowViewModel;
            }
        }

        public static void Start()
        {
            if (_rootScope != null)
            {
                return;
            }

            var builder = new ContainerBuilder();
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            //builder.RegisterAssemblyTypes(assemblies)
            //    .Where(t => typeof(IService).IsAssignableFrom(t))
            //    .SingleInstance()
            //    .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblies)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IOncePerBackend).IsAssignableFrom(t))
                .AsImplementedInterfaces().InstancePerOwned<Func<string, ISqLiteBackendContext>>();

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IViewModel).IsAssignableFrom(t))
                .AsImplementedInterfaces();

            // several view model instances are transitory and created on the fly, if these are tracked by the container then they
            // won't be disposed of in a timely manner

            builder.RegisterAssemblyTypes(assemblies)
                .Where(t => typeof(IViewModel).IsAssignableFrom(t))
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

            _rootScope = builder.Build();
        }

        public static void Stop()
        {
            _rootScope.Dispose();
        }

        //private static T Resolve<T>()
        //{
        //    if (_rootScope == null)
        //    {
        //        throw new Exception("Bootstrapper hasn't been started!");
        //    }

        //    return _rootScope.Resolve<T>(new Parameter[0]);
        //}

        //private static T Resolve<T>(Parameter[] parameters)
        //{
        //    if (_rootScope == null)
        //    {
        //        throw new Exception("Bootstrapper hasn't been started!");
        //    }

        //    return _rootScope.Resolve<T>(parameters);
        //}
    }
}
