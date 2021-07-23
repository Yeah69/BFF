using Autofac;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using Module = Autofac.Module;

namespace BFF.Composition.Wpf.Program
{    
    public class AutofacModule : Module
    {
        public static (View.Wpf.App, IDisposable) ResolveWpfApp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            var container = builder.Build();
            var app = container.BeginLifetimeScope().Resolve<View.Wpf.App>();
            return (app, container);
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var assemblies = AssemblyEnumeration
                .Assemblies
                .Prepend(Assembly.Load(typeof(View.Wpf.AssemblyInfo).Assembly.GetName()))
                .ToArray();

            builder.RegisterModule(new GeneralAutofacModule(assemblies));

            builder.RegisterModule(new PersistenceAutofacModule());

            builder.RegisterModule(new ViewModelAutofacModule());

            builder.RegisterModule(new ViewAutofacModule());

            builder.Register(_ => new CompositeDisposable())
                .InstancePerLifetimeScope();
        }
    }
}