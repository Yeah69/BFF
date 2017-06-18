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

        ReactiveProperty<ICategoryViewModel> ReactiveParent { get; }

        string FullName { get; }
        string GetIndent();
    }

    public class CategoryViewModel : CommonPropertyViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;
        
        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ObservableCollection<ICategoryViewModel> Categories { get; set; } = new ObservableCollection<ICategoryViewModel>();
        
        /*/// <summary>
        /// The Parent
        /// </summary>
        public ICategoryViewModel Parent
        {
            get => _category.Parent == null ? null : Orm.CommonPropertyProvider.GetCategoryViewModel(_category.Parent);
            set
            {
                if(_category.Parent == null && value == null || Orm.CommonPropertyProvider.GetCategoryViewModel(_category.Parent) == value) return;
                _category.Parent = value._category;
                OnUpdate();
                OnPropertyChanged();
            }
        }*/

        public ReactiveProperty<ICategoryViewModel> ReactiveParent { get; }

        public string FullName => $"{(ReactiveParent.Value != null ? $"{ReactiveParent.Value.FullName}." : "")}{Name}";

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
            return $"{ReactiveParent.Value?.GetIndent()}{Name.Value}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetIndent()
        {
            return $"{ReactiveParent.Value?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category, IBffOrm orm, CategoryViewModelService service) : base(orm, category)
        {
            _category = category;

            ReactiveParent =
                _category.ToReactivePropertyAsSynchronized(c => c.Parent, service.GetViewModel, service.GetModel);
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value) && CommonPropertyProvider.IsValidToInsert(this);
        }

        #endregion
    }
}