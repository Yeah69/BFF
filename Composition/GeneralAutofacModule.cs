using Autofac;
using System.Reflection;
using Module = Autofac.Module;

namespace BFF.Composition
{
    public class GeneralAutofacModule : Module
    {

        public GeneralAutofacModule(Assembly[] assemblies)
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
        }
    }
}