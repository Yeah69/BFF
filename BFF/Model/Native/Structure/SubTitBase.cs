using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    /// <summary>
    /// Base class for all SubElement classes, which are used by TitNoTransfer classes
    /// </summary>
    public abstract class SubTitBase : TitLike
    {
        private Category _category;
        private long _sum;

        /// <summary>
        /// This instance is a SubElement of the Parent
        /// </summary>
        [Write(false)]
        public abstract TitNoTransfer Parent{ get; set; }

        /// <summary>
        /// Id of the Parent
        /// </summary>
        public long ParentId
        {
            get { return Parent?.Id ?? -1L; }
            set { }
        }

        /// <summary>
        /// Category of the SubElement
        /// </summary>
        [Write(false)]
        public virtual Category Category
        {
            get { return _category; }
            set
            {
                _category = value;
                if(Id != -1) Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Id of the Category
        /// </summary>
        public long CategoryId
        {
            get { return Category?.Id ?? -1; }
            set { Category = Database?.GetCategory(value); }
        }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public override long Sum
        {
            get { return _sum; }
            set
            {
                if(Parent != null) Parent.Sum -= _sum;
                _sum = value;
                if(Id != -1) Update();
                OnPropertyChanged();
                if(Parent != null) Parent.Sum += _sum;
            }
        }

        /// <summary>
        /// Initializes the SubTitBase-parts of the object
        /// </summary>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTitBase(Category category = null, string memo = null, long sum = 0L) : base(memo: memo)
        {
            ConstrDbLock = true;

            _category = category ?? _category;
            _sum = sum;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        protected SubTitBase(long id, long parentId, long categoryId, long sum, string memo) : base(id, memo)
        {
            ConstrDbLock = true;

            ParentId = parentId;
            CategoryId = categoryId;
            _sum = sum;

            ConstrDbLock = false;
        }

        public override bool ValidToInsert()
        {
            return Category != null && (Database?.AllCategories.Contains(Category) ?? false) && Parent != null;
        }

        [Write(false)]
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            (Parent as ParentTransaction)?.RemoveSubElement(this);
            (Parent as ParentIncome)?.RemoveSubElement(this);
            Parent.Sum -= _sum;
        });

        [Write(false)]
        public string CategoryText { get; set; }

        [Write(false)]
        public Category AddingCategoryParent { get; set; }

        [Write(false)]
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category { Name = CategoryText.Trim() };
            Database?.Insert(newCategory);
            if (AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            OnPropertyChanged(nameof(AllCategories));
            Category = newCategory;
        }, obj =>
        {
            string trimmedCategoryText = CategoryText.Trim();
            return trimmedCategoryText != "" &&
                (AddingCategoryParent == null && Database?.AllCategories?.Count(category => category.Parent == null && category.Name == trimmedCategoryText) == 0 ||
                AddingCategoryParent != null && AddingCategoryParent.Categories.Count(category => category.Name == trimmedCategoryText) == 0);
        });

        [Write(false)]
        public ObservableCollection<Category> AllCategories => Database?.AllCategories;
    }
}
