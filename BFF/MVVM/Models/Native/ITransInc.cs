using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native
{
    public interface ITransInc : IBasicTit
    {
        long AccountId { get; set; }
        DateTime Date { get; set; }
        long PayeeId { get; set; }
        bool Cleared { get; set; }
    }
}
