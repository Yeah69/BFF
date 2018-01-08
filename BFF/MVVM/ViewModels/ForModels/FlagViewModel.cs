using System.Windows.Media;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IFlagViewModel : ICommonPropertyViewModel
    {
        IReactiveProperty<SolidColorBrush> Color { get; }
    }

    public class FlagViewModel : CommonPropertyViewModel, IFlagViewModel
    {

        public FlagViewModel(IFlag flag) : base(flag)
        {

            Color = flag.ToReactivePropertyAsSynchronized(
                f => f.Color,
                color => new SolidColorBrush(color),
                brush => brush.Color, 
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }

        public IReactiveProperty<SolidColorBrush> Color { get; }
    }
}
