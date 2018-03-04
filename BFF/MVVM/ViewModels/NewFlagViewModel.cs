﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
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
        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        IReactiveProperty<string> Text { get; }

        IReactiveProperty<SolidColorBrush> Brush { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        ReactiveCommand AddCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        IObservableReadOnlyList<IFlagViewModel> All { get; }
    }

    public class NewFlagViewModel : INewFlagViewModel, IDisposable
    {
        private readonly IFlagViewModelService _flagViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NewFlagViewModel(
            IHaveFlagViewModel flagOwner,
            Func<Color, IFlag> flagFactory,
            IFlagViewModelService flagViewModelService)
        {
            string ValidatePayeeName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) &&
                    All.All(flag => flag.Name.Value != text.Trim()) 
                    ? null 
                    : "ErrorMessageWrongFlagName".Localize();
            }

            _flagViewModelService = flagViewModelService;

            Text = new ReactiveProperty<string>().SetValidateNotifyError(
                text => ValidatePayeeName(text)).AddTo(_compositeDisposable);

            Brush = new ReactiveProperty<SolidColorBrush>(new SolidColorBrush(Colors.BlueViolet));

            AddCommand = new ReactiveCommand().AddTo(_compositeDisposable);

            AddCommand.Where(
                _ =>
                {
                    (Text as ReactiveProperty<string>)?.ForceValidate();
                    return !Text.HasErrors;
                })
                .Subscribe(_ =>
                {
                    IFlag newFlag = flagFactory(Brush.Value.Color);
                    newFlag.Name = Text.Value.Trim();
                    newFlag.Insert();
                    if(flagOwner != null)
                        flagOwner.Flag.Value = _flagViewModelService.GetViewModel(newFlag);
                }).AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public IReactiveProperty<string> Text { get; }

        public IReactiveProperty<SolidColorBrush> Brush { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ReactiveCommand AddCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IFlagViewModel> All => _flagViewModelService.All;

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}