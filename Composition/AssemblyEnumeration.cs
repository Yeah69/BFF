using System.Collections.Generic;
using System.Reflection;

namespace BFF.Composition
{
    public static class AssemblyEnumeration
    {
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                yield return Assembly.Load(typeof(ViewModel.AssemblyInfo).Assembly.GetName());
                yield return Assembly.Load(typeof(Model.AssemblyInfo).Assembly.GetName());
                yield return Assembly.Load(typeof(Persistence.AssemblyInfo).Assembly.GetName());
                yield return Assembly.Load(typeof(Core.AssemblyInfo).Assembly.GetName());
            }
        }
    }
}