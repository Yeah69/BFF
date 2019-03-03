using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;

namespace BFF.Model.Models
{
    public interface ISummaryAccount : IAccount, IOncePerBackend {}

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    public abstract class SummaryAccount : Account, ISummaryAccount
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount(
            IRxSchedulerProvider rxSchedulerProvider) 
            : base(rxSchedulerProvider, DateTime.MinValue, "All Accounts", 0L) // Placeholder values which are not relevant for the summary
        {
        }
    }
}
