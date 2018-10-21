using System;

namespace BFF.MVVM
{
    interface IVirtualizedRefresh
    {
        event EventHandler PreVirtualizedRefresh;
        void OnPreVirtualizedRefresh();
        event EventHandler PostVirtualizedRefresh;
        void OnPostVirtualizedRefresh();
    }
}
