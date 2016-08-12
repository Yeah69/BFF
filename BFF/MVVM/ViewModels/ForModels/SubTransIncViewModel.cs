using System;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    abstract class SubTransIncViewModel : TitViewModelBase
    {
        protected readonly ISubTransInc SubTransInc;

        #region SubTransaction/SubIncome Properties

        #region Overrides of DbViewModelBase

        public override long Id => SubTransInc.Id;

        #endregion

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

        internal override bool ValidToInsert()
        {
            return Category != null && (Orm?.AllCategories.Contains(Category) ?? false) && SubTransInc.Parent != null;
        }

        #endregion
        protected SubTransIncViewModel(ISubTransInc subTransInc, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
        }

    }
}
