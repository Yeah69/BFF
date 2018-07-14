using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Media;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MoreLinq;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IFlagViewModel : ICommonPropertyViewModel
    {
        SolidColorBrush Color { get; set; }

        IRxRelayCommand<IFlagViewModel> MergeTo { get; }
    }

    public class FlagViewModel : CommonPropertyViewModel, IFlagViewModel
    {
        private readonly IFlag _flag;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public FlagViewModel(
            IFlag flag,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IAccountViewModelService accountViewModelService,
            ISummaryAccountViewModel summaryAccountViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(flag, rxSchedulerProvider)
        {
            _flag = flag;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _accountViewModelService = accountViewModelService;
            _summaryAccountViewModel = summaryAccountViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;

            flag
                .ObservePropertyChanges(nameof(flag.Color))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Color)))
                .AddTo(CompositeDisposable);

            MergeTo = new RxRelayCommand<IFlagViewModel>(cvm =>
            {
                if (cvm is FlagViewModel flagViewModel)
                {
                    flag.MergeTo(flagViewModel._flag);
                }
            });
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    string.Format("ConfirmationDialog_ConfirmFlagDeletion".Localize(), Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(async r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        await base.DeleteAsync();
                        _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                        _summaryAccountViewModel.RefreshTransCollection();
                    }
                    source.SetResult(Unit.Default);
                });
            return source.Task;
        }

        public SolidColorBrush Color
        {
            get => new SolidColorBrush(_flag.Color);
            set => _flag.Color = value.Color;
        }

        public IRxRelayCommand<IFlagViewModel> MergeTo { get; }
    }
}
