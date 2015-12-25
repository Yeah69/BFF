using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Bind<IBffOrm>()
                .To<SqLiteBffOrm>()
                .InSingletonScope()
                .WithConstructorArgument("dbPath", Settings.Default.DBLocation);
        }
    }
}
