using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.IoC;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface INewPayeeViewModel
    {
        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        string? PayeeText { get; set; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        ICommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        IObservableReadOnlyList<IPayeeViewModel>? AllPayees { get; }

        IHavePayeeViewModel? CurrentOwner { get; set; }
    }

    internal class NewPayeeViewModel : NotifyingErrorViewModelBase, INewPayeeViewModel, IScopeInstance, IDisposable
    {
        private readonly ICurrentTextsViewModel _currentTextsViewModel;
        private readonly IPayeeViewModelService _payeeViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new();
        private string? _payeeText;

        public NewPayeeViewModel(
            ICreateNewModels createNewModels,
            ICurrentTextsViewModel currentTextsViewModel,
            IPayeeViewModelService payeeViewModelService)
        {
            _currentTextsViewModel = currentTextsViewModel;
            _payeeViewModelService = payeeViewModelService;

            AddPayeeCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    _compositeDisposable,
                    async () =>
                    {
                        if (!ValidatePayeeName())
                            return;
                        var newPayee = createNewModels.CreatePayee();
                        newPayee.Name = PayeeText?.Trim() ?? String.Empty;
                        await newPayee.InsertAsync();
                        if (CurrentOwner is not null)
                            CurrentOwner.Payee = _payeeViewModelService.GetViewModel(newPayee);
                        CurrentOwner = null;
                        _payeeText = "";
                        OnPropertyChanged(nameof(PayeeText));
                        ClearErrors(nameof(PayeeText));
                        OnErrorChanged(nameof(PayeeText));
                    });
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string? PayeeText
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
        public IObservableReadOnlyList<IPayeeViewModel>? AllPayees => _payeeViewModelService.All;

        public IHavePayeeViewModel? CurrentOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private bool ValidatePayeeName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(PayeeText) &&
                (AllPayees?.All(payee => payee.Name != PayeeText.Trim()) ?? true))
            {
                ClearErrors(nameof(PayeeText));
                ret = true;
            }
            else
            {
                SetErrors(_currentTextsViewModel.CurrentTexts.ErrorMessageWrongPayeeName
                    .ToEnumerable(), nameof(PayeeText));
                ret = false;
            }
            OnErrorChanged(nameof(PayeeText));
            return ret;
        }
    }
}
