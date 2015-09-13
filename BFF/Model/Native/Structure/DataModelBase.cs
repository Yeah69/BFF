using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class DataModelBase
    {
        [Key]
        public abstract long Id { get; set; }
    }
}
