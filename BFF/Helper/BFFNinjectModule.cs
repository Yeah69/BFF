using BFF.DB;
using BFF.DB.SQLite;
using BFF.Properties;
using Ninject.Modules;

namespace BFF.Helper
{
    class BffNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IBffOrm>().To<SqLiteBffOrm>().InSingletonScope();
        }
    }
}
