using System;
using System.Reactive.Disposables;
using BFF.Core.IoC;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using Reactive.Bindings;

namespace BFF.ViewModel.Managers
{
    public interface IParentTransactionFlyoutManager
    {
        IReadOnlyReactiveProperty<IParentTransactionViewModel> OpenParentTransaction { get; }

        void OpenFor(IParentTransactionViewModel parentTransaction);

        void Close();
    }

    internal class ParentTransactionFlyoutManager : IParentTransactionFlyoutManager, IOncePerApplication, IDisposable
    {
        private readonly ReactiveProperty<IParentTransactionViewModel> _openParentTransactionViewModel;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        
        public ParentTransactionFlyoutManager()
        {
            _openParentTransactionViewModel = new ReactiveProperty<IParentTransactionViewModel>(null, ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(_compositeDisposable);
            
        }

        public IReadOnlyReactiveProperty<IParentTransactionViewModel> OpenParentTransaction =>
            _openParentTransactionViewModel;

        public void OpenFor(IParentTransactionViewModel parentTransaction)
        {
            _openParentTransactionViewModel.Value = parentTransaction;
        }

        public void Close()
        {
            _openParentTransactionViewModel.Value = null;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
