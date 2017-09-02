using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.MVVM.Services
{
    public class SubTransactionViewModelService : ModelToViewModelServiceBase<ISubTransaction, ISubTransactionViewModel>
    {
        private readonly Func<ISubTransaction, ISubTransactionViewModel> _subTransactionViewModelFactory;
        private readonly Func<ISubTransaction> _subTransactionFactory;

        public SubTransactionViewModelService(
            Func<ISubTransaction, ISubTransactionViewModel> subTransactionViewModelFactory,
            Func<ISubTransaction> subTransactionFactory)
        {
            _subTransactionViewModelFactory = subTransactionViewModelFactory;
            _subTransactionFactory = subTransactionFactory;
        }

        protected override ISubTransactionViewModel Create(ISubTransaction model)
        {
            return _subTransactionViewModelFactory(model);
        }

        public ISubTransactionViewModel Create(IParentTransaction parent)
        {
            var subTransaction = _subTransactionFactory();
            subTransaction.Parent = parent;

            return _subTransactionViewModelFactory(subTransaction);
        }
    }
}
