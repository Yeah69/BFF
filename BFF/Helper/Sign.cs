using System.Runtime.Serialization;

namespace BFF.Helper
{
    public enum Sign
    {
        [EnumMember(Value = "+")]
        Plus = 0,
        [EnumMember(Value = "-")]
        Minus = 1
    }
}
