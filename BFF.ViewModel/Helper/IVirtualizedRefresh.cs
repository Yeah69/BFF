using System;

namespace BFF.ViewModel.Helper
{
    public interface IVirtualizedRefresh
    {
        event EventHandler PreVirtualizedRefresh;
        void OnPreVirtualizedRefresh();
        event EventHandler PostVirtualizedRefresh;
        void OnPostVirtualizedRefresh();
    }
}
