using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
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

    public class NewAccountViewModel : ViewModelBase, INewAccountViewModel, IDisposable, ITransientViewModel
    {
        private readonly IAccountViewModelService _viewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NewAccountViewModel(
            IAccountRepository repository,
            IAccountViewModelService viewModelService)
        {
            string ValidateName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) &&
                    All.All(a => a.Name.Value != text.Trim()) 
                    ? null 
                    : "ErrorMessageWrongAccountName".Localize<string>();
            }

            _viewModelService = viewModelService;

            Name = new ReactiveProperty<string>().SetValidateNotifyError(
                text => ValidateName(text)).AddTo(_compositeDisposable);

            StartingBalance = new ReactiveProperty<long>(0, ReactivePropertyMode.None).AddTo(_compositeDisposable);

            StartingDate = new ReactiveProperty<DateTime>(DateTime.Today, ReactivePropertyMode.None).AddTo(_compositeDisposable);

            AddCommand = new ReactiveCommand().AddTo(_compositeDisposable);

            AddCommand.Where(
                _ =>
                {
                    (Name as ReactiveProperty<string>)?.ForceValidate();
                    return !Name.HasErrors;
                })
                .Subscribe(_ =>
                {
                    IAccount newAccount = repository.Create();
                    newAccount.Name = Name.Value.Trim();
                    newAccount.StartingBalance = StartingBalance.Value;
                    newAccount.StartingDate = StartingDate.Value;
                    newAccount.Insert();
                    Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
                    Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                }).AddTo(_compositeDisposable);


            Messenger.Default.Register<CultureMessage>(this, message =>
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
            });

            Disposable.Create(() => Messenger.Default.Unregister<CultureMessage>(this)).AddTo(_compositeDisposable);
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
            _compositeDisposable?.Dispose();
        }
    }
}
