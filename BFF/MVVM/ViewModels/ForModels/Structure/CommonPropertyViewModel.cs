using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        string Name { get; set; }
    }

    public abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        private readonly ICommonProperty _commonProperty;

        protected CommonPropertyViewModel(IBffOrm orm, ICommonProperty commonProperty) : base(orm, commonProperty)
        {
            _commonProperty = commonProperty;
        }
        public virtual string Name
        {
            get => _commonProperty.Name;
            set
            {
                if (_commonProperty.Name == value) return;
                OnUpdate();
                _commonProperty.Name = value;
                OnPropertyChanged();
            }
        }
    }
}