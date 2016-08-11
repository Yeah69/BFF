using System;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class SubTransIncViewModel : TitViewModelBase
    {
        protected readonly ISubTransInc SubTransInc;

        #region SubTransaction/SubIncome Properties
        
        public Category Category
        {
            get { return SubTransInc.Category; }
            set
            {
                SubTransInc.Category = value;
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return SubTransInc.Memo; }
            set
            {
                SubTransInc.Memo = value;
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return SubTransInc.Sum; }
            set
            {
                SubTransInc.Sum = value;
                OnPropertyChanged();
            }
        }

        public override bool ValidToInsert()
        {
            return Category != null && (Orm?.AllCategories.Contains(Category) ?? false) && SubTransInc.Parent != null;
        }

        public override void Insert()
        {
            if(SubTransInc is SubTransaction)
                Orm?.Insert(SubTransInc as SubTransaction);
            else if(SubTransInc is SubIncome)
                Orm?.Insert(SubTransInc as SubIncome);
            else
                throw new NotImplementedException($"{SubTransInc.GetType().FullName} is not supported as Subelement."); //todo Localization
        }

        #endregion
        public SubTransIncViewModel(ISubTransInc subTransInc, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
        }

    }
}
