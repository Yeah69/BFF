using System;
using System.Linq;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.Managers
{
    public interface ITransTransformingManager
    {
        IParentTransactionViewModel NotInsertedToParentTransactionViewModel(ITransactionViewModel transactionViewModel);
        ITransferViewModel NotInsertedToTransferViewModel(ITransactionViewModel transactionViewModel);

        ITransactionViewModel NotInsertedToTransactionViewModel(IParentTransactionViewModel parentTransactionViewModel);
        ITransferViewModel NotInsertedToTransferViewModel(IParentTransactionViewModel parentTransactionViewModel);

        ITransactionViewModel NotInsertedToTransactionViewModel(ITransferViewModel transferViewModel);
        IParentTransactionViewModel NotInsertedToParentTransactionViewModel(ITransferViewModel transferViewModel);

        IParentTransactionViewModel InsertedToParentTransactionViewModel(ITransactionViewModel transactionViewModel);

        ITransactionViewModel InsertedToTransactionViewModel(
            IParentTransactionViewModel parentTransactionViewModel, 
            ICategoryBaseViewModel? categoryViewModel);

    }

    internal class TransTransformingManager : ITransTransformingManager, IScopeInstance
    {
        private readonly ICreateNewModels _createNewModels;
        private readonly Func<ITransaction, IAccountBaseViewModel?, ITransactionViewModel> _transactionViewModelFactory;
        private readonly Func<ITransfer, IAccountBaseViewModel?, ITransferViewModel> _transferViewModelFactory;
        private readonly Func<IParentTransaction, IAccountBaseViewModel?, IParentTransactionViewModel> _parentTransactionViewModelFactory;

        public TransTransformingManager(
            ICreateNewModels createNewModels,
            Func<ITransaction, IAccountBaseViewModel?, ITransactionViewModel> transactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel?, ITransferViewModel> transferViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel?, IParentTransactionViewModel> parentTransactionViewModelFactory)
        {
            _createNewModels = createNewModels;
            _transactionViewModelFactory = transactionViewModelFactory;
            _transferViewModelFactory = transferViewModelFactory;
            _parentTransactionViewModelFactory = parentTransactionViewModelFactory;
        }

        public IParentTransactionViewModel NotInsertedToParentTransactionViewModel(ITransactionViewModel transactionViewModel)
        {
            var parentTransactionViewModel = _parentTransactionViewModelFactory(_createNewModels.CreateParentTransaction(), transactionViewModel.Owner);

            parentTransactionViewModel.NewSubTransactionCommand.Execute(null);
            var subTransactionViewModel = parentTransactionViewModel.NewSubTransactions.First();

            parentTransactionViewModel.Date        = transactionViewModel.Date;
            parentTransactionViewModel.Account     = transactionViewModel.Account;
            parentTransactionViewModel.Flag        = transactionViewModel.Flag;
            parentTransactionViewModel.CheckNumber = transactionViewModel.CheckNumber;
            parentTransactionViewModel.Payee       = transactionViewModel.Payee;
            parentTransactionViewModel.Cleared     = transactionViewModel.Cleared;

            subTransactionViewModel.Category       = transactionViewModel.Category;
            subTransactionViewModel.Memo           = transactionViewModel.Memo;
            subTransactionViewModel.Sum.Value      = transactionViewModel.Sum.Value;

            return parentTransactionViewModel;
        }

        public ITransferViewModel NotInsertedToTransferViewModel(ITransactionViewModel transactionViewModel)
        {
            var transferViewModel = _transferViewModelFactory(_createNewModels.CreateTransfer(), transactionViewModel.Owner);

            if (transactionViewModel.Sum.Value <= 0)
                transferViewModel.FromAccount = transactionViewModel.Account;
            else
                transferViewModel.ToAccount   = transactionViewModel.Account;

            transferViewModel.Date        = transactionViewModel.Date;
            transferViewModel.Flag        = transactionViewModel.Flag;
            transferViewModel.CheckNumber = transactionViewModel.CheckNumber;
            transferViewModel.Memo        = transactionViewModel.Memo;
            transferViewModel.Sum.Value   = transactionViewModel.SumAbsolute;
            transferViewModel.Cleared     = transactionViewModel.Cleared;

            return transferViewModel;
        }

        public ITransactionViewModel NotInsertedToTransactionViewModel(IParentTransactionViewModel parentTransactionViewModel)
        {
            var transactionViewModel = _transactionViewModelFactory(_createNewModels.CreateTransaction(), parentTransactionViewModel.Owner);

            transactionViewModel.Date        = parentTransactionViewModel.Date;
            transactionViewModel.Account     = parentTransactionViewModel.Account;
            transactionViewModel.Flag        = parentTransactionViewModel.Flag;
            transactionViewModel.CheckNumber = parentTransactionViewModel.CheckNumber;
            transactionViewModel.Payee       = parentTransactionViewModel.Payee;
            transactionViewModel.Category    = parentTransactionViewModel.NewSubTransactions.FirstOrDefault()?.Category;
            transactionViewModel.Memo        = ExtractMemoFromParentAndNewSubs(parentTransactionViewModel);
            transactionViewModel.Sum.Value   = parentTransactionViewModel.NewSubTransactions.Sum(stvm => stvm.Sum.Value);
            transactionViewModel.Cleared     = parentTransactionViewModel.Cleared;

            return transactionViewModel;
        }

        public ITransferViewModel NotInsertedToTransferViewModel(IParentTransactionViewModel parentTransactionViewModel)
        {
            var transferViewModel = _transferViewModelFactory(_createNewModels.CreateTransfer(), parentTransactionViewModel.Owner);

            if (parentTransactionViewModel.Sum.Value <= 0)
                transferViewModel.FromAccount = parentTransactionViewModel.Account;
            else
                transferViewModel.ToAccount   = parentTransactionViewModel.Account;

            transferViewModel.Date        = parentTransactionViewModel.Date;
            transferViewModel.Flag        = parentTransactionViewModel.Flag;
            transferViewModel.CheckNumber = parentTransactionViewModel.CheckNumber;
            transferViewModel.Memo        = ExtractMemoFromParentAndNewSubs(parentTransactionViewModel);
            transferViewModel.Sum.Value   = parentTransactionViewModel.SumAbsolute;
            transferViewModel.Cleared     = parentTransactionViewModel.Cleared;

            return transferViewModel;
        }

        public ITransactionViewModel NotInsertedToTransactionViewModel(ITransferViewModel transferViewModel)
        {
            var transactionViewModel = _transactionViewModelFactory(_createNewModels.CreateTransaction(), transferViewModel.Owner);

            if (transferViewModel.FromAccount is not null)
            {
                transactionViewModel.Account   = transferViewModel.FromAccount;
                transactionViewModel.Sum.Value = transferViewModel.SumAbsolute * -1L;
            }
            else if (transferViewModel.ToAccount is not null)
            {
                transactionViewModel.Account   = transferViewModel.ToAccount;
                transactionViewModel.Sum.Value = transferViewModel.SumAbsolute;
            }
            else
                transactionViewModel.Sum.Value = transferViewModel.Sum.Value;

            transactionViewModel.Date        = transferViewModel.Date;
            transactionViewModel.Flag        = transferViewModel.Flag;
            transactionViewModel.CheckNumber = transferViewModel.CheckNumber;
            transactionViewModel.Memo        = transferViewModel.Memo;
            transactionViewModel.Cleared     = transferViewModel.Cleared;

            return transactionViewModel;
        }

        public IParentTransactionViewModel NotInsertedToParentTransactionViewModel(ITransferViewModel transferViewModel)
        {
            var parentTransactionViewModel = _parentTransactionViewModelFactory(_createNewModels.CreateParentTransaction(), transferViewModel.Owner);

            parentTransactionViewModel.NewSubTransactionCommand.Execute(null);
            var subTransactionViewModel = parentTransactionViewModel.NewSubTransactions.First();

            if (transferViewModel.FromAccount is not null)
            {
                parentTransactionViewModel.Account = transferViewModel.FromAccount;
                subTransactionViewModel.Sum.Value = transferViewModel.SumAbsolute * -1L;
            }
            else if (transferViewModel.ToAccount is not null)
            {
                parentTransactionViewModel.Account = transferViewModel.ToAccount;
                subTransactionViewModel.Sum.Value = transferViewModel.SumAbsolute;
            }
            else
                subTransactionViewModel.Sum.Value = transferViewModel.Sum.Value;

            parentTransactionViewModel.Date        = transferViewModel.Date;
            parentTransactionViewModel.Flag        = transferViewModel.Flag;
            parentTransactionViewModel.CheckNumber = transferViewModel.CheckNumber;
            parentTransactionViewModel.Cleared     = transferViewModel.Cleared;

            subTransactionViewModel.Memo           = transferViewModel.Memo;

            return parentTransactionViewModel;
        }

        public IParentTransactionViewModel InsertedToParentTransactionViewModel(ITransactionViewModel transactionViewModel)
        {
            return NotInsertedToParentTransactionViewModel(transactionViewModel);
        }

        public ITransactionViewModel InsertedToTransactionViewModel(
            IParentTransactionViewModel parentTransactionViewModel,
            ICategoryBaseViewModel? categoryViewModel)
        {
            var transactionViewModel = NotInsertedToTransactionViewModel(parentTransactionViewModel);

            transactionViewModel.Category  = categoryViewModel;
            transactionViewModel.Memo      = ExtractMemoFromParentAndFlushedSubs(parentTransactionViewModel);
            transactionViewModel.Sum.Value = parentTransactionViewModel.SubTransactions.Sum(stvm => stvm.Sum.Value);

            return transactionViewModel;
        }

        private static string ExtractMemoFromParentAndNewSubs(IParentTransactionViewModel parentTransactionViewModel)
        {
            return string.Join(
                " | ",
                parentTransactionViewModel
                    .Memo
                    .ToEnumerable()
                    .Concat(parentTransactionViewModel
                        .NewSubTransactions
                        .Select(st => st.Memo))
                    .Where(m => m.IsNullOrEmpty().Not()));
        }

        private static string ExtractMemoFromParentAndFlushedSubs(IParentTransactionViewModel parentTransactionViewModel)
        {
            return string.Join(
                " | ",
                parentTransactionViewModel
                    .Memo
                    .ToEnumerable()
                    .Concat(parentTransactionViewModel
                        .SubTransactions
                        .Select(st => st.Memo))
                    .Where(m => m.IsNullOrEmpty().Not()));
        }
    }
}
