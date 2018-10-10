using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.Helper
{
    public interface IConvertFromTransBaseToTransLikeViewModel
    {
        IEnumerable<ITransLikeViewModel> Convert(IEnumerable<ITransBase> transBases, IAccountBaseViewModel owner);
    }

    public class ConvertFromTransBaseToTransLikeViewModel : IConvertFromTransBaseToTransLikeViewModel
    {
        private readonly Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> _transactionViewModelFactory;
        private readonly Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> _parentTransactionViewModelFactory;
        private readonly Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> _transferViewModelFactory;

        public ConvertFromTransBaseToTransLikeViewModel(
            Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory)
        {
            _transactionViewModelFactory = transactionViewModelFactory;
            _parentTransactionViewModelFactory = parentTransactionViewModelFactory;
            _transferViewModelFactory = transferViewModelFactory;
        }

        public IEnumerable<ITransLikeViewModel> Convert(IEnumerable<ITransBase> transBases, IAccountBaseViewModel owner)
        {
            var vmItems = new List<ITransLikeViewModel>();
            foreach (ITransBase transBase in transBases)
            {
                switch (transBase)
                {
                    case ITransfer transfer:
                        vmItems.Add(_transferViewModelFactory(transfer, owner));
                        break;
                    case IParentTransaction parentTransaction:
                        vmItems.Add(_parentTransactionViewModelFactory(parentTransaction, owner));
                        break;
                    case ITransaction transaction:
                        vmItems.Add(_transactionViewModelFactory(transaction, owner));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return vmItems;
        }
    }
}
