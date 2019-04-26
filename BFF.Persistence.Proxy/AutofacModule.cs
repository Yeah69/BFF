using Autofac;

namespace BFF.Persistence.Proxy
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new Persistence.AutofacModule());
        }
    }
}
