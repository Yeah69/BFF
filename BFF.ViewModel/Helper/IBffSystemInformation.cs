using System;
using System.Collections.Generic;
using System.Text;

namespace BFF.ViewModel.Helper
{
    public interface IBffSystemInformation
    {
        double VirtualScreenRight { get; }
        double VirtualScreenLeft { get; }
        double VirtualScreenTop { get; }
        double VirtualScreenBottom { get; }
    }
}
