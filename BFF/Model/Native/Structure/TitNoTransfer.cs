using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    public abstract class TitNoTransfer : TitBase
    {
        [Write(false)]
        public string PayeeText { get; set; }

        [Write(false)]
        public ICommand AddPayeeCommand => new RelayCommand(obj =>
        {
            Database?.Insert(new Payee {Name = PayeeText.Trim()});
        }, obj =>
        {
            string trimmedPayeeText = PayeeText.Trim();
            return trimmedPayeeText != "" && Database?.AllPayees?.Count(payee => payee.Name == trimmedPayeeText) == 0;
        });

        [Write(false)]
        public string CategoryText { get; set; }

        [Write(false)]
        public Category AddingCategoryParent { get; set; }

        [Write(false)]
        public ICommand AddCategoryCommand => new RelayCommand(obj =>
        {
            Category newCategory = new Category {Name = CategoryText.Trim()};
            Database?.Insert(newCategory);
            if (AddingCategoryParent != null)
            {
                newCategory.Parent = AddingCategoryParent;
                AddingCategoryParent.Categories.Add(newCategory);
            }
            OnPropertyChanged(nameof(AllCategories));
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
