using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        string Name { get; set; }
    }

    public abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        protected CommonPropertyViewModel(IBffOrm orm) : base(orm) {}
        public abstract string Name { get; set; }
    }
}