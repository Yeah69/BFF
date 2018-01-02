using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFF.MVVM.ViewModels;

namespace BFF
{
    public interface IBackendContext
    {
        IBudgetOverviewViewModel BudgetOverviewViewModel { get; }
        IAccountTabsViewModel AccountTabsViewModel { get; }
    }

    public abstract class BackendContext : IBackendContext
    {

        protected BackendContext()
        {
        }

        public abstract IBudgetOverviewViewModel BudgetOverviewViewModel { get; }
        public abstract IAccountTabsViewModel AccountTabsViewModel { get; }
    }
}
