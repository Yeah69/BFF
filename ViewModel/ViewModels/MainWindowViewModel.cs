using System;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using BFF.Model.IoC;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.ResXToViewModelGenerator;
using MrMeeseeks.Windows;
using NLog;
using Reactive.Bindings;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels
{
    public interface IMainWindowViewModel : IViewModel
    {
        ICommand NewBudgetPlanCommand { get; }
        ICommand OpenBudgetPlanCommand { get; }
        ICommand CloseBudgetPlanCommand { get; }
        ICommand ParentTransactionOnClose { get; }
        TopLevelViewModelCompositionBase? TopLevelViewModelComposition { get; }
        ICultureManager? CultureManager { get; }
        bool IsEmpty { get; }
        CultureInfo LanguageCulture { get; set; }
        ICurrentTextsViewModel CurrentTextsViewModel { get; } // ToDo 
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string? _title;
        public string? Title
        {
            get => _title;
            private set => SetIfChangedAndRaise(ref _title, value);
        }

        public ICommand NewBudgetPlanCommand { get; }

        public ICommand OpenBudgetPlanCommand { get; }

        public ICommand CloseBudgetPlanCommand { get; }

        public ICommand ParentTransactionOnClose { get; }
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
                customCulture.NumberFormat = CultureManager?.CurrencyCulture.NumberFormat ?? CultureInfo.InvariantCulture.NumberFormat;
                customCulture.DateTimeFormat = CultureManager?.DateCulture.DateTimeFormat ?? CultureInfo.InvariantCulture.DateTimeFormat;

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
        public ICurrentTextsViewModel CurrentTextsViewModel { get; }
        public IReadOnlyReactiveProperty<IParentTransactionViewModel?> OpenParentTransaction { get; }

        public bool ParentTransFlyoutOpen
        {
            get => _parentTransFlyoutOpen;
            set => SetIfChangedAndRaise(ref _parentTransFlyoutOpen, value);
        }

        public MainWindowViewModel(
            ICurrentProject currentProject,
            IContextManager contextManager,
            ICurrentTextsViewModel currentTextsViewModel,
            Func<IContext, ILoadContextViewModel> loadContextViewModelFactory,
            Func<IEmptyContextViewModel> emptyContextViewModelFactory,
            Func<INewFileAccessViewModel> newFileAccessViewModelFactory,
            Func<IOpenFileAccessViewModel> openFileAccessDialogFactory,
            Lazy<IMainWindowDialogManager> mainWindowDialogManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            CompositeDisposable compositeDisposable)
        {
            CurrentTextsViewModel = currentTextsViewModel;
            TransDataGridColumnManager = transDataGridColumnManager;
            _bffSettings = bffSettings;
            Logger.Debug("Initializing …");

            OpenParentTransaction = parentTransactionFlyoutManager.OpenParentTransaction;

            NewBudgetPlanCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    compositeDisposable,
                    async () =>
                    {
                        var newFileAccessViewModel = newFileAccessViewModelFactory();
                        try
                        {
                            var configuration =
                                await mainWindowDialogManager.Value.ShowDialogFor(newFileAccessViewModel);
                            Reset(configuration);
                        }
                        catch (OperationCanceledException)
                        {
                            // cancellation ignored
                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e); // todo error handling
                        }
                    });

            var openBudgetPlanCommand = RxCommand.CanAlwaysExecute().CompositeDisposalWith(compositeDisposable);
            openBudgetPlanCommand
                .Observe
                .SelectMany(async _ =>
                {
                    var openFileAccessViewModel = openFileAccessDialogFactory();
                    try
                    {
                        var configuration = await mainWindowDialogManager.Value.ShowDialogFor(openFileAccessViewModel);
                        Reset(configuration);
                    }
                    catch (OperationCanceledException)
                    {
                        // cancellation ignored
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e); // todo error handling
                    }

                    return Unit.Default;
                })
                .Subscribe(_ => {})
                .CompositeDisposalWith(compositeDisposable);
            

            OpenBudgetPlanCommand = openBudgetPlanCommand;

            CloseBudgetPlanCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    compositeDisposable,
                    currentProject.Close);

            ParentTransactionOnClose = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    compositeDisposable,
                    parentTransactionFlyoutManager.Close);

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
