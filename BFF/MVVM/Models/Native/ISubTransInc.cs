using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransInc : IBasicTit
    {
        long ParentId { get; set; }
    }
}
