using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryViewModel : ICommonPropertyViewModel, IEnumerable<ICategoryViewModel>
    {
        /// <summary>
        /// The Child-Categories
        /// </summary>
        ObservableCollection<ICategoryViewModel> Categories { get; set; }

        /// <summary>
        /// The Parent
        /// </summary>
        ICategoryViewModel Parent { get; set; }

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
        
        /// <summary>
        /// The Parent
        /// </summary>
        public ICategoryViewModel Parent
        {
            get => _category.ParentId == null ? null : Orm.CommonPropertyProvider.GetCategoryViewModel(_category.ParentId ?? 0);
            set
            {
                if(_category.ParentId == null && value == null || Orm.CommonPropertyProvider.GetCategoryViewModel(_category.ParentId ?? 0) == value) return;
                _category.ParentId = value.Id;
                Update();
                OnPropertyChanged();
            }
        }
        
        public string FullName => $"{(Parent != null ? $"{Parent.FullName}." : "")}{Name}";

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
            return $"{Parent?.GetIndent()}{Name}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetIndent()
        {
            return $"{Parent?.GetIndent()}. ";
        }

        public CategoryViewModel(ICategory category, IBffOrm orm) : base(orm, category)
        {
            _category = category;
        }

        #region Overrides of DataModelViewModel

        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name) && CommonPropertyProvider.IsValidToInsert(this);
        }

        protected override void InsertToDb()
        {
            CommonPropertyProvider.Add(_category);
        }

        protected override void UpdateToDb()
        {
            _category.Update(Orm);
        }

        protected override void DeleteFromDb()
        {
            CommonPropertyProvider.Remove(_category);
        }

        #endregion
    }
}