using System;
using BFF.Core.IoC;

namespace BFF.Model.Models
{
    public interface ISummaryAccount : IAccount, IScopeInstance {}

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    public abstract class SummaryAccount : Account, ISummaryAccount
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount() 
            : base(DateTime.MinValue, "All Accounts", 0L) // Placeholder values which are not relevant for the summary
        {
        }
    }
}
