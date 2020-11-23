using System;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.ImportExport;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels;
using NLog;
using Reactive.Bindings;
using IProjectContext = BFF.Model.IoC.IProjectContext;

namespace BFF.ViewModel.ViewModels
{
    public interface IMainWindowViewModel : IViewModel
    {
        IRxRelayCommand NewBudgetPlanCommand { get; }
        IRxRelayCommand OpenBudgetPlanCommand { get; }
        IRxRelayCommand CloseBudgetPlanCommand { get; }
        IRxRelayCommand ParentTransactionOnClose { get; }
        TopLevelViewModelCompositionBase? TopLevelViewModelComposition { get; set; }
        IEmptyViewModel EmptyViewModel { get; }
        ICultureManager? CultureManager { get; }
        bool IsEmpty { get; }
        CultureInfo LanguageCulture { get; set; }
        IReadOnlyReactiveProperty<IParentTransactionViewModel?> OpenParentTransaction { get; }
        bool ParentTransFlyoutOpen { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double X { get; set; }
        double Y { get; set; }
        BffWindowState WindowState { get; set; }
        string? Title { get; }
        ITransDataGridColumnManager TransDataGridColumnManager { get; }
    }

    internal class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IOncePerApplication //todo IDisposable
    {
        private readonly Func<IFileAccessConfiguration, ILoadProjectContext> _loadedProjectContextFactory;
        private readonly Func<IEmptyProjectContext> _emptyContextFactory;
        private readonly IBffSettings _bffSettings;
        private readonly ISetupLocalizationFramework _setupLocalizationFramework;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string? _title;
        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public IRxRelayCommand NewBudgetPlanCommand { get; }

        public IRxRelayCommand OpenBudgetPlanCommand { get; }

        public IRxRelayCommand CloseBudgetPlanCommand { get; }

        public IRxRelayCommand ParentTransactionOnClose { get; }
        public TopLevelViewModelCompositionBase? TopLevelViewModelComposition
        {
            get => _topLevelViewModelComposition;
            set
            {
                if (_topLevelViewModelComposition == value) return;
                _topLevelViewModelComposition = value;
                OnPropertyChanged();
            }
        }

        public ICultureManager? CultureManager
        {
            get => _cultureManager;
            private set
            {
                if (value == _cultureManager) return;
                _cultureManager = value;
                OnPropertyChanged();
            }
        }

        public ITransDataGridColumnManager TransDataGridColumnManager { get; }
        public IEmptyViewModel EmptyViewModel { get; set; }
        public bool IsEmpty => TopLevelViewModelComposition?.AccountTabsViewModel is null 
                               || TopLevelViewModelComposition.BudgetOverviewViewModel is null;

        public CultureInfo LanguageCulture
        {
            get => _bffSettings.Culture_DefaultLanguage;
            set
            {
                _bffSettings.Culture_DefaultLanguage = value;

                CultureInfo customCulture = CultureInfo.CreateSpecificCulture(_bffSettings.Culture_DefaultLanguage.Name);
                customCulture.NumberFormat = CultureManager?.CurrencyCulture.NumberFormat;
                customCulture.DateTimeFormat = CultureManager?.DateCulture.DateTimeFormat;

                _setupLocalizationFramework.With(customCulture);
                Thread.CurrentThread.CurrentCulture = customCulture;
                Thread.CurrentThread.CurrentUICulture = customCulture;

                OnPropertyChanged();
            }
        }

        private const double BorderOffset = 50.0;

        private IParentTransactionViewModel? _parentTransViewModel;

        public IParentTransactionViewModel? ParentTransactionViewModel
        {
            get => _parentTransViewModel;
            set
            {
                _parentTransViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _parentTransFlyoutOpen;
        private readonly SerialDisposable _contextSequence = new SerialDisposable();
        private ICultureManager? _cultureManager;
        private TopLevelViewModelCompositionBase? _topLevelViewModelComposition;

        public IReadOnlyReactiveProperty<IParentTransactionViewModel?> OpenParentTransaction { get; }

        public bool ParentTransFlyoutOpen
        {
            get => _parentTransFlyoutOpen;
            set
            {
                _parentTransFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(
            Func<IFileAccessConfiguration, ILoadProjectContext> loadedProjectContextFactory,
            Func<IFileAccessConfiguration, IProjectContext> newBackendContextFactory,
            Func<IEmptyProjectContext> emptyContextFactory,
            Func<INewFileAccessViewModel> newFileAccessViewModelFactory,
            Func<IOpenFileAccessViewModel> openFileAccessDialogFactory,
            IBffChildWindowManager bffChildWindowManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            IBffSystemInformation bffSystemInformation,
            ISetupLocalizationFramework setupLocalizationFramework,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            IEmptyViewModel emptyViewModel)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
            EmptyViewModel = emptyViewModel;
            _loadedProjectContextFactory = loadedProjectContextFactory;
            _emptyContextFactory = emptyContextFactory;
            _bffSettings = bffSettings;
            _setupLocalizationFramework = setupLocalizationFramework;
            Logger.Debug("Initializing …");
            Reset(null); // todo

            //If the application is not visible on screen, than reset the default position
            //This might occur when one of multiple monitors is switched off or the screen resolution is changed while BFF is off
            if (X - BorderOffset > bffSystemInformation.VirtualScreenRight ||
                Y - BorderOffset > bffSystemInformation.VirtualScreenBottom ||
                X + Width - BorderOffset < bffSystemInformation.VirtualScreenLeft ||
                Y + Height - BorderOffset < bffSystemInformation.VirtualScreenTop)
            {
                X = 50.0;
                Y = 50.0;
            }

            OpenParentTransaction = parentTransactionFlyoutManager.OpenParentTransaction;

            NewBudgetPlanCommand = new RxRelayCommand(() =>
            {
#pragma warning disable 4014
                InnerAsync();
#pragma warning restore 4014

                async Task InnerAsync()
                {
                    var newFileAccessViewModel = newFileAccessViewModelFactory();
                    if (await bffChildWindowManager.OpenOkCancelDialog(newFileAccessViewModel))
                    {
                        try
                        {
                            var configuration = newFileAccessViewModel.GenerateConfiguration();
                            using (var createBackendContext = newBackendContextFactory(configuration))
                            {
                                await createBackendContext
                                    .CreateProject();
                            }
                            Reset(configuration);
                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e); // todo error handling
                        }
                    }
                }
            });

            OpenBudgetPlanCommand = new RxRelayCommand(() =>
            {
#pragma warning disable 4014
                InnerAsync();
#pragma warning restore 4014

                async Task InnerAsync()
                {
                    var openFileAccessViewModel = openFileAccessDialogFactory();
                    if (await bffChildWindowManager.OpenOkCancelDialog(openFileAccessViewModel))
                    {
                        try
                        {
                            var configuration = openFileAccessViewModel.GenerateConfiguration();
                            Reset(configuration);
                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e); // todo error handling
                        }
                    }
                }
            });

            CloseBudgetPlanCommand = new RxRelayCommand(() => Reset(null));

            ParentTransactionOnClose = new RxRelayCommand(parentTransactionFlyoutManager.Close);

            Logger.Trace("Initializing done.");
        }

        private void Reset(IFileAccessConfiguration? config)
        {
            Contexts.IProjectContext context;
            if (config is not null && File.Exists(config.Path))
            {
                var loadedProjectContext = _loadedProjectContextFactory(config);
                _contextSequence.Disposable = loadedProjectContext;
                Title = $"{new FileInfo(config.Path).FullName} - BFF";
                _bffSettings.DBLocation = config.Path;
                context = loadedProjectContext;
            }
            else
            {
                var emptyProject = _emptyContextFactory();
                _contextSequence.Disposable = emptyProject;
                Title = "BFF";
                _bffSettings.DBLocation = "";
                context = emptyProject;
            }

            TopLevelViewModelComposition = context.LoadProject();
            CultureManager = context.CultureManager;

            OnPropertyChanged(nameof(IsEmpty));
        }

        #region SizeLocationWindowState

        public double Width
        {
            get => _bffSettings.MainWindow_Width;
            set
            {
                _bffSettings.MainWindow_Width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _bffSettings.MainWindow_Height;
            set
            {
                _bffSettings.MainWindow_Height = value;
                OnPropertyChanged();
            }
        }

        public double X
        {
            get => _bffSettings.MainWindow_X;
            set
            {
                _bffSettings.MainWindow_X = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _bffSettings.MainWindow_Y;
            set
            {
                _bffSettings.MainWindow_Y = value;
                OnPropertyChanged();
            }
        }

        public BffWindowState WindowState
        {
            get => _bffSettings.MainWindow_WindowState;
            set
            {
                _bffSettings.MainWindow_WindowState = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
