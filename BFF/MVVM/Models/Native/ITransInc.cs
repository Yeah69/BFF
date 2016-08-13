using System;

namespace BFF.MVVM.Models.Native
{
    public interface ITransInc : IBasicTit
    {
        Account Account { get; set; }
        DateTime Date { get; set; }
        Payee Payee { get; set; }
        bool Cleared { get; set; }
    }
}
