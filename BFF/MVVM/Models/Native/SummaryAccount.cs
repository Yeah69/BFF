﻿using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;

namespace BFF.MVVM.Models.Native
{
    public interface ISummaryAccount : IAccount {}

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccount : Account, ISummaryAccount
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount(AccountRepository repository) : base(repository)
        {
            Name = "All Accounts"; //todo Localize! Maybe then override the Name property
        }

        #region Overrides of ExteriorCrudBase

        public override void Insert() {}

        public override void Update() {}

        public override void Delete(){}

        #endregion
    }
}
