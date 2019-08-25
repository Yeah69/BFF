using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface IEditAccountsViewModel
    {
        ReadOnlyObservableCollection<IAccountViewModel> All { get; }

        bool IsDateLong { get; }

        INewAccountViewModel NewAccountViewModel { get; }
    }

    internal class EditAccountsViewModel : ViewModelBase, IEditAccountsViewModel, IDisposable, IOncePerBackend
    {
        private readonly IBffSettings _bffSettings;
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public INewAccountViewModel NewAccountViewModel { get; }
        public ReadOnlyObservableCollection<IAccountViewModel> All { get; }
        public bool IsDateLong => _bffSettings.Culture_DefaultDateLong;

        public EditAccountsViewModel(
            IAccountViewModelService service,
            IBffSettings bffSettings,
            IBackendCultureManager cultureManager,
            INewAccountViewModel newAccountViewModel)
        {
            _bffSettings = bffSettings;
            NewAccountViewModel = newAccountViewModel;
            All = service.All.ToReadOnlyObservableCollection();

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.RefreshDate:
                        OnPropertyChanged(nameof(IsDateLong));
                        break;
                }
            }).AddForDisposalTo(CompositeDisposable);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
