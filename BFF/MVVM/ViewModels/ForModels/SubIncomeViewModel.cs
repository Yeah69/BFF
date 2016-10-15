﻿using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface ISubIncomeViewModel : ISubTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model SubIncome.
    /// </summary>
    class SubIncomeViewModel : SubTransIncViewModel, ISubIncomeViewModel
    {
        /// <summary>
        /// Initializes a SubIncomeViewModel.
        /// </summary>
        /// <param name="subTransInc">A SubIncome Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubIncomeViewModel(ISubIncome subTransInc, IParentTransIncViewModel parent, IBffOrm orm) : 
            base(subTransInc, parent, orm) { }
    }
}