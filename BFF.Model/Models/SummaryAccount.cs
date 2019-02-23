using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface ISummaryAccount : IAccount, IOncePerBackend {}

    /// <summary>
    /// Trans' can be added to an Account
    /// </summary>
    internal class SummaryAccount<TPersistence> : Account<TPersistence>, ISummaryAccount
        where TPersistence : class, IPersistenceModel
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount(
            IRepository<IAccount, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider) : base(null, repository, rxSchedulerProvider, DateTime.MinValue)
        {
            Name = "All Accounts"; //todo Localize! Maybe then override the Name property
        }

        #region Overrides of ExteriorCrudBase

        public override Task InsertAsync() => Task.CompletedTask;

        protected override Task UpdateAsync() => Task.CompletedTask;

        public override Task DeleteAsync() => Task.CompletedTask;

        #endregion
    }
}
