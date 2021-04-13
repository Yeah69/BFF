using System;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using BFF.Model.IoC;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Reactive.Extensions;
using NLog;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    public interface IMainWindowViewModel : IViewModel
    {
        IRxRelayCommand NewBudgetPlanCommand { get; }
        IRxRelayCommand OpenBudgetPlanCommand { get; }
        IRxRelayCommand CloseBudgetPlanCommand { get; }
        IRxRelayCommand ParentTransactionOnClose { get; }
        TopLevelViewModelCompositionBase? TopLevelViewModelComposition { get; }
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
        private readonly IBffSettings _bffSettings;
        private readonly ISetupLocalizationFramework _setupLocalizationFramework;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string? _title;
        public string? Title
        {
            get => _title;
            private set => SetIfChangedAndRaise(ref _title, value);
        }

        public IRxRelayCommand NewBudgetPlanCommand { get; }

        public IRxRelayCommand OpenBudgetPlanCommand { get; }

        public IRxRelayCommand CloseBudgetPlanCommand { get; }

        public IRxRelayCommand ParentTransactionOnClose { get; }
        public TopLevelViewModelCompositionBase? TopLevelViewModelComposition
        {
            get => _topLevelViewModelComposition;
            private set => SetIfChangedAndRaise(ref _topLevelViewModelComposition, value);
        }

        public ICultureManager? CultureManager
        {
            get => _cultureManager;
            private set => SetIfChangedAndRaise(ref _cultureManager, value);
        }

        public ITransDataGridColumnManager TransDataGridColumnManager { get; }
        
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
            set => SetIfChangedAndRaise(ref _parentTransViewModel, value);
        }

        private bool _parentTransFlyoutOpen;
        private ICultureManager? _cultureManager;
        private TopLevelViewModelCompositionBase? _topLevelViewModelComposition;

        public IReadOnlyReactiveProperty<IParentTransactionViewModel?> OpenParentTransaction { get; }

        public bool ParentTransFlyoutOpen
        {
            get => _parentTransFlyoutOpen;
            set => SetIfChangedAndRaise(ref _parentTransFlyoutOpen, value);
        }

        public MainWindowViewModel(
            ICurrentProject currentProject,
            IContextManager contextManager,
            Func<IContext, ILoadContextViewModel> loadContextViewModelFactory,
            Func<IEmptyContextViewModel> emptyContextViewModelFactory,
            Func<INewFileAccessViewModel> newFileAccessViewModelFactory,
            Func<IOpenFileAccessViewModel> openFileAccessDialogFactory,
            IBffChildWindowManager bffChildWindowManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            IBffSystemInformation bffSystemInformation,
            ISetupLocalizationFramework setupLocalizationFramework,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            CompositeDisposable compositeDisposable)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
            _bffSettings = bffSettings;
            _setupLocalizationFramework = setupLocalizationFramework;
            Logger.Debug("Initializing …");

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

            CloseBudgetPlanCommand = new RxRelayCommand(currentProject.Close);

            ParentTransactionOnClose = new RxRelayCommand(parentTransactionFlyoutManager.Close);

            currentProject
                .Current
                .Subscribe(c =>
                {
                    var context = contextManager.Empty == c
                        ? (IContextViewModel)emptyContextViewModelFactory()
                        : loadContextViewModelFactory(c);

                    Title = context.Title;
                    TopLevelViewModelComposition = context.LoadProject();
                    CultureManager = context.CultureManager;

                    OnPropertyChanged(nameof(IsEmpty));
                })
                .CompositeDisposalWith(compositeDisposable);

            Logger.Trace("Initializing done.");
            
            void Reset(IProjectFileAccessConfiguration config)
            {
                if (File.Exists(config.Path))
                {
                    currentProject.CreateAndSet(config);
                }
                else
                {
                    currentProject.Close();
                }
            }
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
