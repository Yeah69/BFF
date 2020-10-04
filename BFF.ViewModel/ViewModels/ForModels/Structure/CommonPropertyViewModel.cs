using System;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        string Name { get; set; }
    }

    internal abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        private readonly ICommonProperty _commonProperty;

        protected CommonPropertyViewModel(
            ICommonProperty commonProperty,
            IRxSchedulerProvider rxSchedulerProvider) : base(commonProperty, rxSchedulerProvider)
        {
            _commonProperty = commonProperty;
            commonProperty
                .ObservePropertyChanged(nameof(commonProperty.Name))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Name)))
                .AddForDisposalTo(CompositeDisposable);
        }
        public virtual string Name
        {
            get => _commonProperty.Name;
            set => _commonProperty.Name = value;

        }
        public override string ToString() => Name;
    }
}