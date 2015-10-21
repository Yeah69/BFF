using System;
using System.Security.RightsManagement;

namespace BFF.Model.Native.Structure
{
    interface ITransactionLike
    {
        Type Type { get; }
    }
}
