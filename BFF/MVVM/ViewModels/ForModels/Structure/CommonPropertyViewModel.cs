using System;
using System.Reactive.Linq;
using BFF.Core;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Helper.Extensions;
using BFF.Model.Models.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        string Name { get; set; }
    }

    public abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        private readonly ICommonProperty _commonProperty;

        protected CommonPropertyViewModel(
            ICommonProperty commonProperty,
            IRxSchedulerProvider rxSchedulerProvider) : base(commonProperty, rxSchedulerProvider)
        {
            _commonProperty = commonProperty;
            commonProperty
                .ObservePropertyChanges(nameof(commonProperty.Name))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Name)))
                .AddTo(CompositeDisposable);
        }
        public virtual string Name
        {
            get => _commonProperty.Name;
            set => _commonProperty.Name = value;

        }
        public override string ToString() => Name;
    }
}