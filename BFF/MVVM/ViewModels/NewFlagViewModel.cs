using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;
using System.Windows.Media;
using BFF.Core.IoCMarkerInterfaces;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public interface INewFlagViewModel
    {
        string Text { get; set; }

        IReactiveProperty<SolidColorBrush> Brush { get; }
        
        ICommand AddCommand { get; }
        
        IObservableReadOnlyList<IFlagViewModel> All { get; }

        IHaveFlagViewModel CurrentOwner { get; set; }
    }

    public class NewFlagViewModel : NotifyingErrorViewModelBase, INewFlagViewModel, IOncePerBackend, IDisposable
    {
        private readonly IFlagViewModelService _flagViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private string _text;

        public NewFlagViewModel(
            Func<Color, IFlag> flagFactory,
            IFlagViewModelService flagViewModelService)
        {

            _flagViewModelService = flagViewModelService;

            Brush = new ReactiveProperty<SolidColorBrush>(new SolidColorBrush(Colors.BlueViolet));

            AddCommand = new AsyncRxRelayCommand(async () =>
            {
                if (!ValidateName()) return;
                IFlag newFlag = flagFactory(Brush.Value.Color);
                newFlag.Name = Text.Trim();
                await newFlag.InsertAsync();
                if (CurrentOwner != null)
                    CurrentOwner.Flag = _flagViewModelService.GetViewModel(newFlag);
                CurrentOwner = null;
                _text = "";
                OnPropertyChanged(nameof(Text));
                ClearErrors(nameof(Text));
                OnErrorChanged(nameof(Text));
            }).AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public string Text
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

        public IReactiveProperty<SolidColorBrush> Brush { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IFlagViewModel> All => _flagViewModelService.All;

        public IHaveFlagViewModel CurrentOwner { get; set; }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        private bool ValidateName()
        {
            bool ret;
            if (!string.IsNullOrWhiteSpace(Text) &&
                All.All(f => f.Name != Text.Trim()))
            {
                ClearErrors(nameof(Text));
                ret = true;
            }
            else
            {
                SetErrors("ErrorMessageWrongFlagName".Localize().ToEnumerable(), nameof(Text));
                ret = false;
            }
            OnErrorChanged(nameof(Text));
            return ret;
        }
    }
}
