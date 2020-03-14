using Autofac;

namespace BFF.Persistence.Proxy
{
    public class AutoFacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new Persistence.AutoFacModule());
        }
    }
}
