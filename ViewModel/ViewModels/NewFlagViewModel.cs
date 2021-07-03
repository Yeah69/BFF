using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Windows;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    public interface INewFlagViewModel
    {
        string? Text { get; set; }

        IReactiveProperty<Color> Brush { get; }
        
        ICommand AddCommand { get; }
        
        IObservableReadOnlyList<IFlagViewModel>? All { get; }

        IHaveFlagViewModel? CurrentOwner { get; set; }
    }

    internal class NewFlagViewModel : NotifyingErrorViewModelBase, INewFlagViewModel, IOncePerBackend, IDisposable
    {
        private readonly IFlagViewModelService _flagViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new();
        private string? _text;

        public NewFlagViewModel(
            ICreateNewModels createNewModels,
            IFlagViewModelService flagViewModelService)
        {
            _flagViewModelService = flagViewModelService;

            Brush = new ReactiveProperty<Color>(Color.BlueViolet);

            AddCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    _compositeDisposable,
                    async () =>
                    {
                        if (!ValidateName()) return;
                        IFlag newFlag = createNewModels.CreateFlag();
                        newFlag.Color = Brush.Value;
                        newFlag.Name = Text?.Trim() ?? String.Empty;
                        await newFlag.InsertAsync();
                        if (CurrentOwner is not null)
                            CurrentOwner.Flag = _flagViewModelService.GetViewModel(newFlag);
                        CurrentOwner = null;
                        _text = "";
                        OnPropertyChanged(nameof(Text));
                        ClearErrors(nameof(Text));
                        OnErrorChanged(nameof(Text));
                    });
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string? Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged();
                ValidateName();
            }
        }

        public IReactiveProperty<Color> Brush { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IFlagViewModel>? All => _flagViewModelService.All;

        public IHaveFlagViewModel? CurrentOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private bool ValidateName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(Text) &&
                (All?.All(f => f.Name != Text.Trim()) ?? true))
            {
                ClearErrors(nameof(Text));
                ret = true;
            }
            else
            {
                SetErrors("" // ToDo _localizer.Localize("ErrorMessageWrongFlagName")
                    .ToEnumerable(), nameof(Text));
                ret = false;
            }
            OnErrorChanged(nameof(Text));
            return ret;
        }
    }
}
