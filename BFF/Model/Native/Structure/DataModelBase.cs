using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class DataModelBase
    {
        [Write(false)]
        public abstract string CreateTableStatement { get; }

        [Key]
        public abstract int ID { get; set; }
    }
}
