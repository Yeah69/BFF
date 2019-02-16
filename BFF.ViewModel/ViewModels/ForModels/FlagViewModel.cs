using System;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MoreLinq;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IFlagViewModel : ICommonPropertyViewModel
    {
        Color Color { get; set; }
        void MergeTo(IFlagViewModel target);
        bool CanMergeTo(IFlagViewModel target);
    }

    public class FlagViewModel : CommonPropertyViewModel, IFlagViewModel
    {
        private readonly IFlag _flag;
        private readonly ILocalizer _localizer;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public FlagViewModel(
            IFlag flag,
            ILocalizer localizer,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            IAccountViewModelService accountViewModelService,
            ISummaryAccountViewModel summaryAccountViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(flag, rxSchedulerProvider)
        {
            _flag = flag;
            _localizer = localizer;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;
            _accountViewModelService = accountViewModelService;
            _summaryAccountViewModel = summaryAccountViewModel;
            _rxSchedulerProvider = rxSchedulerProvider;

            flag
                .ObservePropertyChanges(nameof(flag.Color))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Color)))
                .AddTo(CompositeDisposable);
        }

        public override Task DeleteAsync()
        {
            TaskCompletionSource<Unit> source = new TaskCompletionSource<Unit>();
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmFlagDeletion"), Name),
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

        public Color Color
        {
            get => _flag.Color;
            set => _flag.Color = value;
        }

        public void MergeTo(IFlagViewModel target)
        {
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    _localizer.Localize("ConfirmationDialog_Title"),
                    string.Format(_localizer.Localize("ConfirmationDialog_ConfirmFlagMerge"), Name, target.Name),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(r =>
                {
                    if (r != BffMessageDialogResult.Affirmative) return;

                    if (target is FlagViewModel flagViewModel)
                    {
                        Observable
                            .FromAsync(token => _flag.MergeToAsync(flagViewModel._flag), _rxSchedulerProvider.Task)
                            .ObserveOn(_rxSchedulerProvider.UI)
                            .Subscribe(_ =>
                            {
                                _summaryAccountViewModel.RefreshTransCollection();
                                _accountViewModelService.All.ForEach(avm => avm.RefreshTransCollection());
                            });
                    }
                });
        }

        public bool CanMergeTo(IFlagViewModel target)
        {
            return target.Name != Name;
        }
    }
}
