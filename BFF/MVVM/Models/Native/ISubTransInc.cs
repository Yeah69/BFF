using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface ISubTransInc : IBasicTit
    {
        TitNoTransfer Parent { get; set; }
    }
}
