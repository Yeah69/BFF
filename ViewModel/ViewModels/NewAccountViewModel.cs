﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
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
        string? Name { get; set; }

        IReactiveProperty<long> StartingBalance { get; }

        IReactiveProperty<DateTime> StartingDate { get; }

        bool ShowLongDate { get; }

        /// <summary>
        /// Creates a new Account.
        /// </summary>
        ICommand AddCommand { get; }
    }

    internal class NewAccountViewModel : NotifyingErrorViewModelBase, INewAccountViewModel, IDisposable
    {
        private readonly IBffSettings _bffSettings;
        private readonly ICurrentTextsViewModel _currentTextsViewModel;
        private readonly IAccountViewModelService _viewModelService;

        protected readonly CompositeDisposable CompositeDisposable = new();

        private string? _name;

        public NewAccountViewModel(
            ICreateNewModels createNewModels,
            IBffSettings bffSettings,
            ICurrentTextsViewModel currentTextsViewModel,
            IBackendCultureManager cultureManager,
            ISummaryAccountViewModel summaryAccountViewModel,
            IAccountViewModelService viewModelService)
        {
            _bffSettings = bffSettings;
            _currentTextsViewModel = currentTextsViewModel;
            _viewModelService = viewModelService;

            StartingBalance = new ReactiveProperty<long>(0, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            StartingDate = new ReactiveProperty<DateTime>(DateTime.Today, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            AddCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    CompositeDisposable,
                    async () =>
                    {
                        if (!ValidateName()) return;
                        var newAccount = createNewModels.CreateAccount();
                        newAccount.Name = Name?.Trim() ?? String.Empty;
                        newAccount.StartingBalance = StartingBalance.Value;
                        newAccount.StartingDate = StartingDate.Value;
                        await newAccount.InsertAsync();
                        var newAccountViewModel = viewModelService.GetViewModel(newAccount) ??
                                                  throw new NullReferenceException("Shouldn't be null");
                        newAccountViewModel.IsOpen = true;
                        summaryAccountViewModel.RefreshStartingBalance();
                        summaryAccountViewModel.RefreshBalance();
                        _name = "";
                        OnPropertyChanged(nameof(Name));
                        ClearErrors(nameof(Name));
                        OnErrorChanged(nameof(Name));
                    });

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
        public string? Name
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
        public IObservableReadOnlyList<IAccountViewModel>? All => _viewModelService.All;

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }

        private bool ValidateName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(Name) &&
                (All?.All(f => f.Name != Name.Trim()) ?? false))
            {
                ClearErrors(nameof(Name));
                ret = true;
            }
            else
            {
                SetErrors(_currentTextsViewModel.CurrentTexts.ErrorMessageWrongAccountName
                    .ToEnumerable(), nameof(Name));
                ret = false;
            }
            OnErrorChanged(nameof(Name));
            return ret;
        }
    }
}
