using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModelRepositories;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryViewModel : ICommonPropertyViewModel, IEnumerable<ICategoryViewModel>
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ObservableCollection<ICategoryViewModel> Categories { get; }

        /// <summary>
        /// The Parent
        /// </summary>
        //ICategoryViewModel Parent { get; set; }

        ReactiveProperty<ICategoryViewModel> Parent { get; }

        string FullName { get; }
        string GetIndent();
    }

    public class CategoryViewModel : CommonPropertyViewModel, ICategoryViewModel
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ObservableCollection<ICategoryViewModel> Categories { get; } = new ObservableCollection<ICategoryViewModel>();
        
        /// <summary>
        /// The Parent
        /// </summary>
        public ReactiveProperty<ICategoryViewModel> Parent { get; }

        private ICategoryViewModel _previousParent = null;

        public string FullName => $"{(Parent.Value != null ? $"{Parent.Value.FullName}." : "")}{Name}";

        public IEnumerator<ICategoryViewModel> GetEnumerator()
        {
            yield return this;

            foreach(ICategoryViewModel categoryViewModel in Categories)
            {
                using(IEnumerator<ICategoryViewModel> enumerator = categoryViewModel.GetEnumerator())
                {
                    while(enumerator.MoveNext())
                        yield return enumerator.Current;
                }

            }
        }

        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Name with preceding dots (foreach Ancestor one)</returns>
        public override string ToString()
        {
            return $"{Parent.Value?.GetIndent()}{Name.Value}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetIndent()
        {
            return $"{Parent.Value?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category, IBffOrm orm, CategoryViewModelService service) : base(orm, category)
        {
            Parent = category.ToReactivePropertyAsSynchronized(
                                 c => c.Parent, 
                                 c => service.GetViewModel(c as Category) as ICategoryViewModel, 
                                 cvm => service.GetModel(cvm as CategoryViewModel))
                             .AddTo(CompositeDisposable);
            
            Parent.Subscribe(cvm =>
            {
                if(_previousParent?.Categories.Contains(this) ?? false)
                    _previousParent.Categories.Remove(this);
                if(cvm != null && !cvm.Categories.Contains(this))
                    cvm.Categories.Add(this);
                _previousParent = cvm;
            }).AddTo(CompositeDisposable);
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value) && CommonPropertyProvider.IsValidToInsert(this);
        }

        #endregion
    }
}