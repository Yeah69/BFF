using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface INewAccountViewModel
    {
        /// <summary>
        /// User input of to be created Account.
        /// </summary>
        string Name { get; set; }

        IReactiveProperty<long> StartingBalance { get; }

        IReactiveProperty<DateTime> StartingDate { get; }

        bool ShowLongDate { get; }

        /// <summary>
        /// Creates a new Account.
        /// </summary>
        ICommand AddCommand { get; }
    }

    public class NewAccountViewModel : NotifyingErrorViewModelBase, INewAccountViewModel, IDisposable
    {
        private readonly ILocalizer _localizer;
        private readonly IBffSettings _bffSettings;
        private readonly IAccountViewModelService _viewModelService;

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        private string _name;

        public NewAccountViewModel(
            ICreateNewModels createNewModels,
            ILocalizer localizer,
            IBffSettings bffSettings,
            ICultureManager cultureManager,
            ISummaryAccountViewModel summaryAccountViewModel,
            IAccountViewModelService viewModelService)
        {
            _localizer = localizer;
            _bffSettings = bffSettings;
            _viewModelService = viewModelService;

            StartingBalance = new ReactiveProperty<long>(0, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            StartingDate = new ReactiveProperty<DateTime>(DateTime.Today, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            AddCommand = new AsyncRxRelayCommand(async () =>
            {
                if (!ValidateName()) return;
                IAccount newAccount = createNewModels.CreateAccount();
                newAccount.Name = Name.Trim();
                newAccount.StartingBalance = StartingBalance.Value;
                newAccount.StartingDate = StartingDate.Value;
                await newAccount.InsertAsync();
                viewModelService.GetViewModel(newAccount).IsOpen = true;
                summaryAccountViewModel.RefreshStartingBalance();
                summaryAccountViewModel.RefreshBalance();
                _name = "";
                OnPropertyChanged(nameof(Name));
                ClearErrors(nameof(Name));
                OnErrorChanged(nameof(Name));
            }).AddTo(CompositeDisposable);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                    case CultureMessage.RefreshCurrency:
                    case CultureMessage.RefreshDate:
                        StartingDate.Value = StartingDate.Value;
                        OnPropertyChanged(nameof(ShowLongDate));
                        StartingBalance.Value = StartingBalance.Value;
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
                ValidateName();
            }
        }

        public IReactiveProperty<long> StartingBalance { get; }

        public IReactiveProperty<DateTime> StartingDate { get; }

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool ShowLongDate => _bffSettings.Culture_DefaultDateLong;

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// All currently available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel> All => _viewModelService.All;

        public void Dispose()
        {
            CompositeDisposable?.Dispose();
        }

        private bool ValidateName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(Name) &&
                All.All(f => f.Name != Name.Trim()))
            {
                ClearErrors(nameof(Name));
                ret = true;
            }
            else
            {
                SetErrors(_localizer.Localize("ErrorMessageWrongAccountName").ToEnumerable(), nameof(Name));
                ret = false;
            }
            OnErrorChanged(nameof(Name));
            return ret;
        }
    }
}
