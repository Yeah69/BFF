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
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models.Utility;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MuVaViMo;
using org.mariuszgromada.math.mxparser;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.Dialogs
{
    public interface IImportCsvBankStatementViewModel : IOkCancelDialogViewModel
    {
        IObservableReadOnlyList<ICsvBankStatementImportProfileViewModel> Profiles { get; }

        IList<ICsvBankStatementImportItemViewModel> Items { get; }

        IReactiveProperty<ICsvBankStatementImportProfileViewModel> SelectedProfile { get; }

        IReactiveProperty<string> FilePath { get; }
        IReadOnlyReactiveProperty<bool>  FileExists { get; }
        IReadOnlyReactiveProperty<string> Header { get; }
        bool HeaderDoMatch { get; }

        IReactiveProperty<bool> ShowItemsError { get; }

        IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }

        IRxRelayCommand BrowseCsvBankStatementFileCommand { get; }

        IRxRelayCommand DeselectProfileCommand { get; }
    }

    internal class ImportCsvBankStatementViewModel : OkCancelDialogViewModel, IImportCsvBankStatementViewModel
    {
        public ImportCsvBankStatementViewModel(
            Func<IReactiveProperty<string>,
            ICsvBankStatementImportNonProfileViewModel> nonProfileViewModelFactory,
            Func<IBffOpenFileDialog> openFileDialogFactory,
            IRxSchedulerProvider schedulerProvider,
            IBffSettings bffSettings,
            ILocalizer localizer,
            Func<(DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum), ICsvBankStatementImportItemViewModel> createItem,
            ICsvBankStatementProfileManager csvBankStatementProfileManger,
            Func<ICsvBankStatementImportProfile, ICsvBankStatementImportProfileViewModel> profileViewModelFactory)
        {
            Profiles = csvBankStatementProfileManger.Profiles.Transform(profileViewModelFactory);
            
            FilePath = new ReactivePropertySlim<string>(
                "",
                ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(CompositeDisposable);

            var nonProfileViewModel = nonProfileViewModelFactory(FilePath).AddForDisposalTo(CompositeDisposable);
            Configuration = new ReactivePropertySlim<ICsvBankStatementImportNonProfileViewModel>(nonProfileViewModel, ReactivePropertyMode.DistinctUntilChanged);

            SelectedProfile = new ReactivePropertySlim<ICsvBankStatementImportProfileViewModel>(Profiles.FirstOrDefault(cbsipvm => cbsipvm.Name.Value == bffSettings.SelectedCsvProfile), ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(CompositeDisposable);
            SelectedProfile
                .Subscribe(sp =>
                {
                    Configuration.Value = sp ?? nonProfileViewModel;
                    bffSettings.SelectedCsvProfile = !(sp is ICsvBankStatementImportProfileViewModel cbsipvm)
                        ? null
                        : cbsipvm.Name.Value;
                })
                .AddForDisposalTo(CompositeDisposable);

            

            Profiles
                .ToCollectionChanged<ICsvBankStatementImportProfileViewModel>()
                .Where(cc => cc.Action == NotifyCollectionChangedAction.Add)
                .Select(cc => cc.Value)
                .Subscribe(pvm => SelectedProfile.Value = pvm)
                .AddForDisposalTo(CompositeDisposable);

            FileExists = new ReadOnlyReactivePropertySlim<bool>(
                FilePath.Select(File.Exists), 
                mode: ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(CompositeDisposable);

            Header = new ReadOnlyReactivePropertySlim<string>(
                FilePath.Select(path => File.Exists(path) ? File.ReadLines(path, Encoding.Default).FirstOrDefault() : ""), 
                mode: ReactivePropertyMode.DistinctUntilChanged).AddForDisposalTo(CompositeDisposable);

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
                .AddForDisposalTo(CompositeDisposable);

            Configuration
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(items => OnPropertyChanged(nameof(HeaderDoMatch)))
                .AddForDisposalTo(CompositeDisposable);

            Header
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                .AddForDisposalTo(CompositeDisposable);

            SerialDisposable configurationPropertyChanges = new SerialDisposable().AddForDisposalTo(CompositeDisposable);
            SerialDisposable configurationHeaderChanges = new SerialDisposable().AddForDisposalTo(CompositeDisposable);

            Configuration
                .Where(configuration => configuration != null)
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
                        .AssignTo(configurationPropertyChanges);
                    configuration.Header
                        .ObserveOn(schedulerProvider.UI)
                        .Subscribe(_ =>
                        {
                            OnPropertyChanged(nameof(HeaderDoMatch));
                        })
                        .AssignTo(configurationHeaderChanges);
                })
                .AddForDisposalTo(CompositeDisposable);


            BrowseCsvBankStatementFileCommand = new RxRelayCommand(() =>
                {
                    var bffOpenFileDialog = openFileDialogFactory();
                    bffOpenFileDialog.Multiselect = false;
                    bffOpenFileDialog.DefaultExt = "csv";
                    bffOpenFileDialog.Filter = $"{localizer.Localize("Domain_BankStatement")} (*.csv)|*.csv";
                    bffOpenFileDialog.FileName = "";
                    bffOpenFileDialog.Title = localizer.Localize("Domain_OpenBankStatement");
                    if (bffOpenFileDialog.ShowDialog() == true)
                    {
                        FilePath.Value = bffOpenFileDialog.FileName;
                    }
                })
                .AddForDisposalTo(CompositeDisposable);

            DeselectProfileCommand = new RxRelayCommand(() => SelectedProfile.Value = null)
                .AddForDisposalTo(CompositeDisposable);

            ShowItemsError = new ReactivePropertySlim<bool>(false, ReactivePropertyMode.DistinctUntilChanged);

            IList<ICsvBankStatementImportItemViewModel> ExtractItems()
            {
                IList<ICsvBankStatementImportItemViewModel> ret = new List<ICsvBankStatementImportItemViewModel>();

                try
                {
                    if (FileExists.Value && HeaderDoMatch)
                    {
                        var indexToSegment = Configuration.Value.Segments.Value.Select((s, i) => (s, i))
                            .ToDictionary(_ => _.i, _ => _.s);
                        ret = File.ReadLines(FilePath.Value).Skip(1).Select(line =>
                        {
                            var segmentValues = line
                                .Split(Configuration.Value.Delimiter.Value)
                                .Select((v, i) => (v, i))
                                .ToDictionary(_ => indexToSegment[_.i], _ => _.v);
                            var payeeString = Configuration.Value.PayeeFormat.Value;
                            var memoString = Configuration.Value.MemoFormat.Value;
                            var sumString = Configuration.Value.SumFormula.Value;
                            var date = DateTime.TryParse(
                                segmentValues[Configuration.Value.DateSegment.Value],
                                Configuration.Value.DateLocalization.Value, 
                                DateTimeStyles.AllowWhiteSpaces, 
                                out DateTime dateResult)
                                ? dateResult
                                : (DateTime?) null;

                            foreach (var segment in Configuration.Value.Segments.Value)
                            {
                                payeeString = payeeString.Replace($"{{{segment}}}", segmentValues[segment].Trim('"'));
                                memoString = memoString.Replace($"{{{segment}}}", segmentValues[segment].Trim('"'));

                                if (sumString.Contains($"{{{segment}}}"))
                                {
                                    var sumPartParsingSuccess = double.TryParse(segmentValues[segment], NumberStyles.Any, Configuration.Value.SumLocalization.Value, out var sumPartResult);
                                    long sum = (long) Math.Round((sumPartParsingSuccess ? sumPartResult : 0.0) *
                                                      Math.Pow(10, Configuration.Value.SumLocalization.Value.NumberFormat.CurrencyDecimalDigits));
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
                                    ? (long?) null
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
        public IList<ICsvBankStatementImportItemViewModel> Items { get; private set; }
        public IReactiveProperty<ICsvBankStatementImportProfileViewModel> SelectedProfile { get; }
        public IReactiveProperty<string> FilePath { get; }
        public IReadOnlyReactiveProperty<bool> FileExists { get; }
        public IReadOnlyReactiveProperty<string> Header { get; }
        public IReactiveProperty<bool> ShowItemsError { get; }
        public IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }
        public IRxRelayCommand BrowseCsvBankStatementFileCommand { get; }
        public IRxRelayCommand DeselectProfileCommand { get; }

        public bool HeaderDoMatch => Configuration.Value != null && Header.Value == Configuration.Value.Header.Value;
    }
}
