using System.Diagnostics;
using System.Reflection;
using Autofac;
using BFF.Core.IoC;
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
        }
    }
}
