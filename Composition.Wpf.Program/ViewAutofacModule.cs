using Autofac;
using Module = Autofac.Module;

namespace BFF.Composition.Wpf.Program
{
    public class ViewAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }
    }
}