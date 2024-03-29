﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using BFF.Core.Helper;
using BFF.Model.Models.Utility;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
using MuVaViMo;
using org.mariuszgromada.math.mxparser;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IImportCsvBankStatementViewModel : IMainWindowDialogContentViewModel<IList<ICsvBankStatementImportItemViewModel>>
    {
        IObservableReadOnlyList<ICsvBankStatementImportProfileViewModel> Profiles { get; }

        IList<ICsvBankStatementImportItemViewModel> Items { get; }

        IReactiveProperty<ICsvBankStatementImportProfileViewModel?> SelectedProfile { get; }

        IReactiveProperty<string> FilePath { get; }
        IReadOnlyReactiveProperty<bool>  FileExists { get; }
        IReadOnlyReactiveProperty<string> Header { get; }
        bool HeaderDoMatch { get; }

        IReactiveProperty<bool> ShowItemsError { get; }

        IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }

        ICommand BrowseCsvBankStatementFileCommand { get; }

        ICommand DeselectProfileCommand { get; }
        ICommand OkCommand { get; }
    }

    public class ImportCsvBankStatementViewModel : MainWindowDialogContentViewModel<IList<ICsvBankStatementImportItemViewModel>>, IImportCsvBankStatementViewModel
    {
        public ImportCsvBankStatementViewModel(
            Func<IReactiveProperty<string>,
            ICsvBankStatementImportNonProfileViewModel> nonProfileViewModelFactory,
            Func<IBffOpenFileDialog> openFileDialogFactory,
            ICurrentTextsViewModel currentTextsViewModel,
            IRxSchedulerProvider schedulerProvider,
            IBffSettings bffSettings,
            Func<(DateTime? Date, string? Payee, bool CreatePayeeIfNotExisting, string? Memo, long? Sum), ICsvBankStatementImportItemViewModel> createItem,
            ICsvBankStatementProfileManager csvBankStatementProfileManger,
            Func<ICsvBankStatementImportProfile, ICsvBankStatementImportProfileViewModel> profileViewModelFactory) 
            : base(currentTextsViewModel.CurrentTexts.CsvBankStatementImport_Title)
        {
            Profiles = csvBankStatementProfileManger.Profiles.Transform(profileViewModelFactory);
            
            FilePath = new ReactivePropertySlim<string>(
                "",
                ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(CompositeDisposable);

            var nonProfileViewModel = nonProfileViewModelFactory(FilePath).CompositeDisposalWith(CompositeDisposable);
            Configuration = new ReactivePropertySlim<ICsvBankStatementImportNonProfileViewModel>(nonProfileViewModel, ReactivePropertyMode.DistinctUntilChanged);

            SelectedProfile = new ReactivePropertySlim<ICsvBankStatementImportProfileViewModel?>(Profiles.FirstOrDefault(cbsipvm => cbsipvm.Name.Value == bffSettings.SelectedCsvProfile), ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(CompositeDisposable);
            SelectedProfile
                .Subscribe(sp =>
                {
                    Configuration.Value = sp ?? nonProfileViewModel;
                    bffSettings.SelectedCsvProfile = !(sp is { } cbsipvm)
                        ? null
                        : cbsipvm.Name.Value;
                })
                .CompositeDisposalWith(CompositeDisposable);

            

            Profiles
                .ToCollectionChanged<ICsvBankStatementImportProfileViewModel>()
                .Where(cc => cc.Action == NotifyCollectionChangedAction.Add)
                .Select(cc => cc.Value)
                .Subscribe(pvm => SelectedProfile.Value = pvm)
                .CompositeDisposalWith(CompositeDisposable);

            FileExists = new ReadOnlyReactivePropertySlim<bool>(
                FilePath.Select(File.Exists), 
                mode: ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(CompositeDisposable);

            Header = new ReadOnlyReactivePropertySlim<string>(
                FilePath.Select(path => File.Exists(path) ? File.ReadLines(path, Encoding.Default).FirstOrDefault() ?? "" : ""), 
                mode: ReactivePropertyMode.DistinctUntilChanged).CompositeDisposalWith(CompositeDisposable);

            ShowItemsError = new ReactivePropertySlim<bool>(false, ReactivePropertyMode.DistinctUntilChanged);
            
            Configuration.Select(_ => Unit.Default)
                .Merge(FilePath.Select(_ => Unit.Default))
                .ObserveOn(schedulerProvider.Task)
                .Select(_ => ExtractItems())
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(items =>
                {
                    Items = items;
                    OnPropertyChanged(nameof(Items));
                })
                .CompositeDisposalWith(CompositeDisposable);

            Configuration
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                .CompositeDisposalWith(CompositeDisposable);

            Header
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                .CompositeDisposalWith(CompositeDisposable);

            SerialDisposable configurationPropertyChanges = new SerialDisposable().CompositeDisposalWith(CompositeDisposable);
            SerialDisposable configurationHeaderChanges = new SerialDisposable().CompositeDisposalWith(CompositeDisposable);

            Configuration
                .Where(configuration => configuration is not null)
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(configuration =>
                {
                    Observable.Merge(
                        configuration.Header.PropertyChangedAsObservable(),
                        configuration.DateLocalization.PropertyChangedAsObservable(),
                        configuration.DateSegment.PropertyChangedAsObservable(),
                        configuration.Delimiter.PropertyChangedAsObservable(),
                        configuration.MemoFormat.PropertyChangedAsObservable(),
                        configuration.PayeeFormat.PropertyChangedAsObservable(),
                        configuration.ShouldCreateNewPayeeIfNotExisting.PropertyChangedAsObservable(),
                        configuration.SumFormula.PropertyChangedAsObservable(),
                        configuration.SumLocalization.PropertyChangedAsObservable())
                        .ObserveOn(schedulerProvider.Task)
                        .Select(_ => ExtractItems())
                        .ObserveOn(schedulerProvider.UI)
                        .Subscribe(items =>
                        {
                            Items = items;
                            OnPropertyChanged(nameof(Items));
                        })
                        .SerializeDisposalWith(configurationPropertyChanges);
                    configuration.Header
                        .ObserveOn(schedulerProvider.UI)
                        .Subscribe(_ =>
                        {
                            OnPropertyChanged(nameof(HeaderDoMatch));
                        })
                        .SerializeDisposalWith(configurationHeaderChanges);
                })
                .CompositeDisposalWith(CompositeDisposable);


            BrowseCsvBankStatementFileCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () =>
                    {
                        var bffOpenFileDialog = openFileDialogFactory();
                        bffOpenFileDialog.Multiselect = false;
                        bffOpenFileDialog.DefaultExt = "csv";
                        bffOpenFileDialog.Filter = currentTextsViewModel.CurrentTexts.Domain_BankStatement;
                        bffOpenFileDialog.FileName = "";
                        bffOpenFileDialog.Title = currentTextsViewModel.CurrentTexts.Domain_OpenBankStatement;
                        if (bffOpenFileDialog.ShowDialog() == true)
                        {
                            FilePath.Value = bffOpenFileDialog.FileName;
                        }
                    });
            
            DeselectProfileCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => SelectedProfile.Value = null);
            
            var okCommand = RxCommand.CanAlwaysExecute().CompositeDisposalWith(CompositeDisposable);

            okCommand
                .Observe
                .Subscribe(_ => TaskCompletionSource.SetResult(Items))
                .CompositeDisposalWith(CompositeDisposable);

            this.OkCommand = okCommand;

            IList<ICsvBankStatementImportItemViewModel> ExtractItems()
            {
                IList<ICsvBankStatementImportItemViewModel> ret = new List<ICsvBankStatementImportItemViewModel>();

                try
                {
                    if (FileExists.Value && HeaderDoMatch)
                    {
                        var indexToSegment = Configuration
                            .Value
                            .Segments
                            .Value
                            ?.Select((s, i) => (s, i))
                            .ToDictionary(_ => _.i, _ => _.s)
                            ?? new Dictionary<int, string>();
                        ret = File.ReadLines(FilePath.Value).Skip(1).Select(line =>
                        {
                            var segmentValues = line
                                .Split(Configuration.Value.Delimiter?.Value ?? ' ')
                                .Select((v, i) => (v, i))
                                .ToDictionary(_ => indexToSegment[_.i], _ => _.v);
                            var payeeString = Configuration.Value.PayeeFormat.Value;
                            var memoString = Configuration.Value.MemoFormat.Value;
                            var sumString = Configuration.Value.SumFormula.Value;
                            var date = Configuration.Value.DateSegment.Value is { } dateSegmentValue
                                       && DateTime.TryParse(
                                            segmentValues[dateSegmentValue],
                                            Configuration.Value.DateLocalization.Value, 
                                            DateTimeStyles.AllowWhiteSpaces, 
                                            out DateTime dateResult)
                                        ? dateResult
                                        : (DateTime?) null;

                            foreach (var segment in Configuration.Value.Segments.Value ?? Enumerable.Empty<string>())
                            {
                                payeeString = payeeString.Replace($"{{{segment}}}", segmentValues[segment].Trim('"'));
                                memoString = memoString.Replace($"{{{segment}}}", segmentValues[segment].Trim('"'));

                                if (sumString.Contains($"{{{segment}}}"))
                                {
                                    var sumPartParsingSuccess = double.TryParse(segmentValues[segment], NumberStyles.Any, Configuration.Value.SumLocalization.Value, out var sumPartResult);
                                    long sum = (long) Math.Round((sumPartParsingSuccess ? sumPartResult : 0.0) *
                                                      Math.Pow(10, Configuration.Value.SumLocalization.Value?.NumberFormat.CurrencyDecimalDigits ?? 1));
                                    sumString = sumString.Replace($"{{{segment}}}", sum.ToString());
                                }

                            }

                            return createItem( 
                                (date,
                                payeeString.IsWhitespace() 
                                    ? null 
                                    : payeeString,
                                Configuration.Value.ShouldCreateNewPayeeIfNotExisting.Value,
                                memoString.IsWhitespace()
                                    ? null 
                                    : memoString,
                                sumString.IsWhitespace()
                                    ? null
                                    : (int)new Expression(sumString).calculate()) );
                        }).ToList();
                    }
                    
                    ShowItemsError.Value = false;
                }
                catch (Exception)
                {
                    ShowItemsError.Value = true;
                }

                return ret;
            }
        }

        public IObservableReadOnlyList<ICsvBankStatementImportProfileViewModel> Profiles { get; }

        public IList<ICsvBankStatementImportItemViewModel> Items { get; private set; } =
            new List<ICsvBankStatementImportItemViewModel>();
        public IReactiveProperty<ICsvBankStatementImportProfileViewModel?> SelectedProfile { get; }
        public IReactiveProperty<string> FilePath { get; }
        public IReadOnlyReactiveProperty<bool> FileExists { get; }
        public IReadOnlyReactiveProperty<string> Header { get; }
        public IReactiveProperty<bool> ShowItemsError { get; }
        public IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }
        public ICommand BrowseCsvBankStatementFileCommand { get; }
        public ICommand DeselectProfileCommand { get; }
        public ICommand OkCommand { get; }

        public bool HeaderDoMatch => Configuration.Value is not null && Header.Value == Configuration.Value.Header.Value;
    }
}
