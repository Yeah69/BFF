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
        private readonly IFlag _flag;
        private readonly IFlagViewModelService _service;

        public FlagViewModel(IFlag flag, IBffOrm orm, IFlagViewModelService service) : base(orm, flag)
        {
            _flag = flag;
            _service = service;

            Color = flag.ToReactivePropertyAsSynchronized(
                f => f.Color,
                color => new SolidColorBrush(color),
                brush => brush.Color, 
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value) && _service.All.All(f => f.Name.Value != Name.Value) && _flag != Flag.Default;
        }

        #endregion

        public IReactiveProperty<SolidColorBrush> Color { get; }
    }
}
