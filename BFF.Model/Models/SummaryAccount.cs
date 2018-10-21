using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Repositories.ModelRepositories;

namespace BFF.Model.Models
{
    public interface ISummaryAccount : IAccount, IOncePerBackend {}

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    internal class SummaryAccount : Account, ISummaryAccount
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount(
            IAccountRepository repository,
            IRxSchedulerProvider rxSchedulerProvider) : base(repository, rxSchedulerProvider, DateTime.MinValue)
        {
            Name = "All Accounts"; //todo Localize! Maybe then override the Name property
        }

        #region Overrides of ExteriorCrudBase

        public override Task InsertAsync() => Task.CompletedTask;

        public override Task UpdateAsync() => Task.CompletedTask;

        public override Task DeleteAsync() => Task.CompletedTask;

        #endregion
    }
}
