using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public interface INewAccountViewModel
    {
        /// <summary>
        /// User input of to be created Account.
        /// </summary>
        IReactiveProperty<string> Name { get; }

        IReactiveProperty<long> StartingBalance { get; }

        IReactiveProperty<DateTime> StartingDate { get; }

        bool IsDateFormatLong { get; }

        /// <summary>
        /// Creates a new Account.
        /// </summary>
        ReactiveCommand AddCommand { get; }
    }

    public class NewAccountViewModel : ViewModelBase, INewAccountViewModel, IDisposable
    {
        private readonly IAccountViewModelService _viewModelService;

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public NewAccountViewModel(
            Func<IAccount> factory,
            ICultureManager cultureManager,
            IAccountViewModelService viewModelService)
        {
            string ValidateName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) &&
                    All.All(a => a.Name.Value != text.Trim()) 
                    ? null 
                    : "ErrorMessageWrongAccountName".Localize();
            }

            _viewModelService = viewModelService;
            
            Name = new ReactiveProperty<string>("", ReactivePropertyMode.DistinctUntilChanged)
                .SetValidateNotifyError(text => ValidateName(text))
                .AddTo(CompositeDisposable);

            StartingBalance = new ReactiveProperty<long>(0, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            StartingDate = new ReactiveProperty<DateTime>(DateTime.Today, ReactivePropertyMode.None).AddTo(CompositeDisposable);

            AddCommand = new ReactiveCommand().AddTo(CompositeDisposable);

            AddCommand.Where(
                _ =>
                {
                    (Name as ReactiveProperty<string>)?.ForceValidate();
                    return !Name.HasErrors;
                })
                .Subscribe(_ =>
                {
                    IAccount newAccount = factory();
                    newAccount.Name = Name.Value.Trim();
                    newAccount.StartingBalance = StartingBalance.Value;
                    newAccount.StartingDate = StartingDate.Value;
                    newAccount.InsertAsync();
                    Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
                    Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                }).AddTo(CompositeDisposable);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                    case CultureMessage.RefreshCurrency:
                    case CultureMessage.RefreshDate:
                        StartingDate.Value = StartingDate.Value;
                        OnPropertyChanged(nameof(IsDateFormatLong));
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
        public IReactiveProperty<string> Name { get; }

        public IReactiveProperty<long> StartingBalance { get; }

        public IReactiveProperty<DateTime> StartingDate { get; }

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool IsDateFormatLong => Settings.Default.Culture_DefaultDateLong;

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ReactiveCommand AddCommand { get; }

        /// <summary>
        /// All currently available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel> All => _viewModelService.All;

        public void Dispose()
        {
            CompositeDisposable?.Dispose();
        }
    }
}
