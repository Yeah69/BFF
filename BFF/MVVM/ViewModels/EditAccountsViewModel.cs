
using System;
using System.Collections.ObjectModel;
using BFF.DB;
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

    public class EditAccountsViewModel : ObservableObject, IEditAccountsViewModel, IDisposable, IOncePerBackend
    {
        public INewAccountViewModel NewAccountViewModel { get; }
        public ReadOnlyObservableCollection<IAccountViewModel> All { get; }
        public bool IsDateLong => Settings.Default.Culture_DefaultDateLong;

        public EditAccountsViewModel(
            IAccountViewModelService service,
            INewAccountViewModel newAccountViewModel)
        {
            NewAccountViewModel = newAccountViewModel;
            All = service.All.ToReadOnlyObservableCollection();
            
            Messenger.Default.Register<CultureMessage>(this, message =>
            {
                switch (message)
                {
                    case CultureMessage.RefreshDate:
                        OnPropertyChanged(nameof(IsDateLong));
                        break;
                }
            });
        }

        public void Dispose()
        {
            Messenger.Default.Unregister<CultureMessage>(this);
        }
    }
}
