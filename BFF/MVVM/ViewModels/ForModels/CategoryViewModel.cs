using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ICategoryViewModel : IDataModelViewModel, IEnumerable<ICategoryViewModel>
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
        string Name { get; set; }
        string GetIndent();
    }

    public class CategoryViewModel : DataModelViewModel, ICategoryViewModel
    {
        private readonly ICategory _category;

        public string Name
        {
            get { return _category.Name; }
            set
            {
                if(_category.Name == value) return;
                Update();
                _category.Name = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// The Child-Categories
        /// </summary>
        public ObservableCollection<ICategoryViewModel> Categories { get; set; } = new ObservableCollection<ICategoryViewModel>();
        
        /// <summary>
        /// The Parent
        /// </summary>
        public ICategoryViewModel Parent
        {
            get { return Orm.CommonPropertyProvider.GetCategoryViewModel(_category.ParentId ?? 0); }
            set
            {
                if(Orm.CommonPropertyProvider.GetCategoryViewModel(_category.ParentId ?? 0) == value) return;
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

        public CategoryViewModel(ICategory category, IBffOrm orm) : base(orm)
        {
            _category = category;
        }

        #region Overrides of DataModelViewModel

        public override long Id => _category.Id;

        public override bool ValidToInsert()
        {
            throw new System.NotImplementedException();
        }

        protected override void InsertToDb()
        {
            Orm?.CommonPropertyProvider.Add(_category);
        }

        protected override void UpdateToDb()
        {
            _category.Update(Orm);
        }

        protected override void DeleteFromDb()
        {
            Orm?.CommonPropertyProvider.Remove(_category);
        }

        #endregion
    }
}