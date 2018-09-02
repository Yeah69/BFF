using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native.Utility;
using BFF.MVVM.ViewModels.ForModels.Utility;
using Microsoft.Win32;
using MuVaViMo;
using NCalc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.Dialogs
{
    public interface IImportCsvBankStatementViewModel
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

        IReactiveProperty IsOpen { get; }

        IRxRelayCommand BrowseCsvBankStatementFileCommand { get; }

        IRxRelayCommand DeselectProfileCommand { get; }

        IRxRelayCommand OkCommand { get; }

        IRxRelayCommand CancelCommand { get; }
    }

    public class ImportCsvBankStatementViewModel : ViewModelBase, IImportCsvBankStatementViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public ImportCsvBankStatementViewModel(
            Func<IReactiveProperty<string>, 
            ICsvBankStatementImportNonProfileViewModel> nonProfileViewModelFactory,
            Action<IList<ICsvBankStatementImportItemViewModel>> onOk,
            IRxSchedulerProvider schedulerProvider,
            Func<(DateTime? Date, string Payee, bool CreatePayeeIfNotExisting, string Memo, long? Sum), ICsvBankStatementImportItemViewModel> createItem,
            ICsvBankStatementProfileManager csvBankStatementProfileManger,
            Func<ICsvBankStatementImportProfile, ICsvBankStatementImportProfileViewModel> profileViewModelFactory)
        {
            IsOpen = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged);

            Profiles = csvBankStatementProfileManger.Profiles.Transform(profileViewModelFactory);
            
            FilePath = new ReactivePropertySlim<string>(
                "",
                ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            var nonProfileViewModel = nonProfileViewModelFactory(FilePath).AddHere(_compositeDisposable);
            Configuration = new ReactivePropertySlim<ICsvBankStatementImportNonProfileViewModel>(nonProfileViewModel, ReactivePropertyMode.DistinctUntilChanged);

            SelectedProfile = new ReactivePropertySlim<ICsvBankStatementImportProfileViewModel>(null, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SelectedProfile
                .Subscribe(sp => Configuration.Value = sp ?? nonProfileViewModel)
                .AddHere(_compositeDisposable);

            

            Profiles
                .ToCollectionChanged<ICsvBankStatementImportProfileViewModel>()
                .Where(cc => cc.Action == NotifyCollectionChangedAction.Add)
                .Select(cc => cc.Value)
                .Subscribe(pvm => SelectedProfile.Value = pvm)
                .AddHere(_compositeDisposable);

            FileExists = new ReadOnlyReactivePropertySlim<bool>(
                FilePath.Select(File.Exists), 
                mode: ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            Header = new ReadOnlyReactivePropertySlim<string>(
                FilePath.Select(path => File.Exists(path) ? File.ReadLines(path, Encoding.Default).FirstOrDefault() : ""), 
                mode: ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

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
                .AddHere(_compositeDisposable);

            Configuration
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(items => OnPropertyChanged(nameof(HeaderDoMatch)))
                .AddHere(_compositeDisposable);

            Header
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                .AddHere(_compositeDisposable);

            SerialDisposable configurationPropertyChanges = new SerialDisposable().AddHere(_compositeDisposable);
            SerialDisposable configurationHeaderChanges = new SerialDisposable().AddHere(_compositeDisposable);

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
                .AddHere(_compositeDisposable);


            BrowseCsvBankStatementFileCommand = new RxRelayCommand(() =>
                {
                    OpenFileDialog openFileDialog =
                        new OpenFileDialog
                        {
                            Multiselect = false,
                            DefaultExt = "csv",
                            Filter = $"{"Domain_BankStatement".Localize()} (*.csv)|*.csv",
                            FileName = "",
                            Title = "Domain_OpenBankStatement".Localize()
                        };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        FilePath.Value = openFileDialog.FileName;
                    }
                })
                .AddHere(_compositeDisposable);

            DeselectProfileCommand = new RxRelayCommand(() => SelectedProfile.Value = null)
                .AddHere(_compositeDisposable);

            OkCommand = new RxRelayCommand(()  =>
                {
                    IsOpen.Value = false;
                    onOk(Items);
                    _compositeDisposable.Dispose();
                })
                .AddHere(_compositeDisposable);

            CancelCommand = new RxRelayCommand(() =>
                {
                    IsOpen.Value = false;
                    _compositeDisposable.Dispose();
                })
                .AddHere(_compositeDisposable);

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
                                payeeString.IsNullOrWhiteSpace() 
                                    ? null 
                                    : payeeString,
                                Configuration.Value.ShouldCreateNewPayeeIfNotExisting.Value,
                                memoString.IsNullOrWhiteSpace()
                                    ? null 
                                    : memoString,
                                sumString.IsNullOrWhiteSpace()
                                    ? (long?) null
                                    : (int)new Expression(sumString).Evaluate()) );
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
        public IReactiveProperty IsOpen { get; }
        public IRxRelayCommand BrowseCsvBankStatementFileCommand { get; }
        public IRxRelayCommand DeselectProfileCommand { get; }
        public IRxRelayCommand OkCommand { get; }
        public IRxRelayCommand CancelCommand { get; }

        public bool HeaderDoMatch => Configuration.Value != null && Header.Value == Configuration.Value.Header.Value;
    }
}
