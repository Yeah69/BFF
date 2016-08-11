using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IBasicTit
    {
        long Id { get; }
        Category Category { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
}
