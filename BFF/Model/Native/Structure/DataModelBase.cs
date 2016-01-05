using BFF.DB;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class DataModelBase : ObservableObject
    {
        public virtual long Id { get; set; }

        [Write(false)]
        public static IBffOrm Database { get; set; }
    }
}
