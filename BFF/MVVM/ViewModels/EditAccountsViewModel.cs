using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using BFF.Core.IoCMarkerInterfaces;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IEditAccountsViewModel
    {
        ReadOnlyObservableCollection<IAccountViewModel> All { get; }

        bool IsDateLong { get; }

        INewAccountViewModel NewAccountViewModel { get; }
    }

    public class EditAccountsViewModel : ViewModelBase, IEditAccountsViewModel, IDisposable, IOncePerBackend
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public INewAccountViewModel NewAccountViewModel { get; }
        public ReadOnlyObservableCollection<IAccountViewModel> All { get; }
        public bool IsDateLong => Settings.Default.Culture_DefaultDateLong;

        public EditAccountsViewModel(
            IAccountViewModelService service,
            IBackendCultureManager cultureManager,
            INewAccountViewModel newAccountViewModel)
        {
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
            }).AddHere(CompositeDisposable);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
