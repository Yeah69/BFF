﻿using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryViewModel : ICommonPropertyViewModel
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        /// <summary>
        /// The Parent
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Parent { get; }

        string FullName { get; }
        int Depth { get; }
        string GetIndent();
    }

    public class CategoryViewModel : CommonPropertyViewModel, ICategoryViewModel
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }
        
        /// <summary>
        /// The Parent
        /// </summary>
        public IReactiveProperty<ICategoryViewModel> Parent { get; }

        public string FullName => $"{(Parent.Value != null ? $"{Parent.Value.FullName}." : "")}{Name.Value}";

        public int Depth => Parent.Value?.Depth + 1 ?? 0;

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString()
        {
            return Name.Value;
        }

        public string GetIndent()
        {
            return $"{Parent.Value?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category, IBffOrm orm, CategoryViewModelService service) : base(orm, category)
        {
            Parent = category.ToReactivePropertyAsSynchronized(
                                 c => c.Parent, 
                                 service.GetViewModel, 
                                 service.GetModel)
                             .AddTo(CompositeDisposable);

            Categories = category.Categories.ToReadOnlyReactiveCollection(service.GetViewModel).AddTo(CompositeDisposable);
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value) && CommonPropertyProvider.IsValidToInsert(this);
        }

        #endregion
    }
}