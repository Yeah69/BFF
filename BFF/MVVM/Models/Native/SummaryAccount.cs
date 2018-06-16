using System;
using System.Threading.Tasks;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;

namespace BFF.MVVM.Models.Native
{
    public interface ISummaryAccount : IAccount, IOncePerBackend {}

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccount : Account, ISummaryAccount
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
