using BFF.DB;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class DataModelBase : ObservableObject
    {
        [Key]
        public long Id { get; set; } = -1;

        [Write(false)]
        public static IBffOrm Database { get; set; }
    }
}
