using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.Dialogs
{
    public interface IImportCsvBankStatementViewModel
    {
        IObservableReadOnlyList<ICsvBankStatementImportProfileViewModel> Profiles { get; }

        IList<CsvBankStatementImportItem> Items { get; }

        IReactiveProperty<ICsvBankStatementImportProfileViewModel> SelectedProfile { get; }

        IReactiveProperty<string> FilePath { get; }
        IReadOnlyReactiveProperty<bool>  FileExists { get; }
        IReadOnlyReactiveProperty<string> Header { get; }
        bool HeaderDoMatch { get; }


        IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }

        ReactiveCommand BrowseCsvBankStatementFileCommand { get; }

        ReactiveCommand DeselectProfileCommand { get; }
    }

    public class ImportCsvBankStatementViewModel : ObservableObject, IImportCsvBankStatementViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public ImportCsvBankStatementViewModel(
            Func<IReactiveProperty<string>, 
            ICsvBankStatementImportNonProfileViewModel> nonProfileViewModelFactory,
            IRxSchedulerProvider schedulerProvider,
            ICsvBankStatementProfileManager csvBankStatementProfileManger,
            Func<ICsvBankStatementImportProfile, ICsvBankStatementImportProfileViewModel> profileViewModelFactory)
        {
            Profiles = csvBankStatementProfileManger.Profiles.Transform(profileViewModelFactory);
            
            FilePath = new ReactivePropertySlim<string>(
                "",
                ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            var nonProfileViewModel = nonProfileViewModelFactory(FilePath).AddHere(_compositeDisposable);
            Configuration = new ReactivePropertySlim<ICsvBankStatementImportNonProfileViewModel>(nonProfileViewModel, ReactivePropertyMode.DistinctUntilChanged);

            SelectedProfile = new ReactivePropertySlim<ICsvBankStatementImportProfileViewModel>(null, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);
            SelectedProfile.Subscribe(sp => Configuration.Value = sp ?? nonProfileViewModel);

            

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

            Configuration.Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)));

            Header.ObserveOn(schedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                .AddHere(_compositeDisposable);

            SerialDisposable serialDisposable = new SerialDisposable();

            Configuration
                .Where(configuration => configuration != null)
                .Subscribe(configuration => configuration
                    .Header
                    .Subscribe(_ => OnPropertyChanged(nameof(HeaderDoMatch)))
                    .AssignTo(serialDisposable))
                .AddHere(_compositeDisposable);


            BrowseCsvBankStatementFileCommand.Subscribe(_ =>
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
            });

            DeselectProfileCommand.Subscribe(_ => SelectedProfile.Value = null).AddHere(_compositeDisposable);

        }

        public IObservableReadOnlyList<ICsvBankStatementImportProfileViewModel> Profiles { get; }
        public IList<CsvBankStatementImportItem> Items { get; }
        public IReactiveProperty<ICsvBankStatementImportProfileViewModel> SelectedProfile { get; }
        public IReactiveProperty<string> FilePath { get; }
        public IReadOnlyReactiveProperty<bool> FileExists { get; }
        public IReadOnlyReactiveProperty<string> Header { get; }
        public IReactiveProperty<ICsvBankStatementImportNonProfileViewModel> Configuration { get; }
        public ReactiveCommand BrowseCsvBankStatementFileCommand { get; } = new ReactiveCommand();
        public ReactiveCommand DeselectProfileCommand { get; } = new ReactiveCommand();

        public bool HeaderDoMatch => Configuration.Value != null && Header.Value == Configuration.Value.Header.Value;
    }
}
