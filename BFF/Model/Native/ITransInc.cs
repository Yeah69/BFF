using System;

namespace BFF.Model.Native
{
    interface ITransInc : ISubTransInc
    {
        Account Account { get; set; }
        DateTime Date { get; set; }
        Payee Payee { get; set; }
        bool Cleared { get; set; }
    }
}
