using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public interface IParentTransactionViewModelService : IModelToViewModelServiceBase<IParentTransaction, IParentTransactionViewModel>
    {
    }

    public class ParentTransactionViewModelService : ModelToViewModelServiceBase<IParentTransaction, IParentTransactionViewModel>, IParentTransactionViewModelService
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
