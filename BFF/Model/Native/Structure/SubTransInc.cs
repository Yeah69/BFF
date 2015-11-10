using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    abstract class SubTransInc : TransactionLike
    {
        [Write(false)]
        public abstract TitBase Parent { get; set; }

        // todo: Find a way for setter of ParentId
        public abstract long ParentId { get; }

        [Write(false)]
        public abstract Category Category { get; set; }

        public abstract long CategoryId { get; set; }

        public abstract string Memo { get; set; }
        
        public abstract long Sum { get; set; }
    }
}
