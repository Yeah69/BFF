using System;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class ParentTransactionViewModelService : ModelToViewModelServiceBase<IParentTransaction, IParentTransactionViewModel>
    {
        private readonly Func<IParentTransaction, IParentTransactionViewModel> _parentTransactionViewModelFactory;

        public ParentTransactionViewModelService(
            Func<IParentTransaction, IParentTransactionViewModel> parentTransactionViewModelFactory)
        {
            _parentTransactionViewModelFactory = parentTransactionViewModelFactory;
        }

        protected override IParentTransactionViewModel Create(IParentTransaction model)
        {
            return _parentTransactionViewModelFactory(model);
        }
    }
}
