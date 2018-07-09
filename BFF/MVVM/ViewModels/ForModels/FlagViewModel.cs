using System;
using System.Reactive.Linq;
using System.Windows.Media;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

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

        public FlagViewModel(
            IFlag flag,
            IRxSchedulerProvider rxSchedulerProvider) : base(flag, rxSchedulerProvider)
        {
            _flag = flag;

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

        public SolidColorBrush Color
        {
            get => new SolidColorBrush(_flag.Color);
            set => _flag.Color = value.Color;
        }

        public IRxRelayCommand<IFlagViewModel> MergeTo { get; }
    }
}
