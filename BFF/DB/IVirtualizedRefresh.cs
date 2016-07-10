using System;

namespace BFF.DB
{
    interface IVirtualizedRefresh
    {
        event EventHandler PreVirtualizedRefresh;
        void OnPreVirtualizedRefresh();
        event EventHandler PostVirtualizedRefresh;
        void OnPostVirtualizedRefresh();
    }
}
