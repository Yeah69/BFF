using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface INewPayeeViewModel
    {
        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        string PayeeText { get; set; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        ICommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        IObservableReadOnlyList<IPayeeViewModel> AllPayees { get; }

        IHavePayeeViewModel CurrentOwner { get; set; }
    }

    public class NewPayeeViewModel : NotifyingErrorViewModelBase, INewPayeeViewModel, IOncePerBackend, IDisposable
    {
        private readonly ILocalizer _localizer;
        private readonly IPayeeViewModelService _payeeViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private string _payeeText;

        public NewPayeeViewModel(
            Func<IPayee> payeeFactory,
            ILocalizer localizer,
            IPayeeViewModelService payeeViewModelService)
        {
            _localizer = localizer;
            _payeeViewModelService = payeeViewModelService;

            AddPayeeCommand = new AsyncRxRelayCommand(async () =>
            {
                if (!ValidatePayeeName())
                    return;
                IPayee newPayee = payeeFactory();
                newPayee.Name = PayeeText.Trim();
                await newPayee.InsertAsync();
                if (CurrentOwner != null)
                    CurrentOwner.Payee = _payeeViewModelService.GetViewModel(newPayee);
                CurrentOwner = null;
                _payeeText = "";
                OnPropertyChanged(nameof(PayeeText));
                ClearErrors(nameof(PayeeText));
                OnErrorChanged(nameof(PayeeText));
            }).AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string PayeeText
        {
            get => _payeeText;
            set
            {
                if (_payeeText == value) return;
                _payeeText = value;
                OnPropertyChanged();
                ValidatePayeeName();
            }
        }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ICommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IPayeeViewModel> AllPayees => _payeeViewModelService.All;

        public IHavePayeeViewModel CurrentOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private bool ValidatePayeeName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(PayeeText) &&
                AllPayees.All(payee => payee.Name != PayeeText.Trim()))
            {
                ClearErrors(nameof(PayeeText));
                ret = true;
            }
            else
            {
                SetErrors(_localizer.Localize("ErrorMessageWrongPayeeName").ToEnumerable(), nameof(PayeeText));
                ret = false;
            }
            OnErrorChanged(nameof(PayeeText));
            return ret;
        }
    }
}
