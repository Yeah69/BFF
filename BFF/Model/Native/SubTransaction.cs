using System;
using BFF.Helper.Import;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.Model.Native
{
    /// <summary>
    /// A SubElement of a Transaction
    /// </summary>
    public class SubTransaction : SubTitBase, ISubTransInc
    {
        private TitNoTransfer _parent;

        public static implicit operator SubTransaction(YNAB.Transaction ynabTransaction)
        {
            Category tempCategory = Category.GetOrCreate(ynabTransaction.Category);
            SubTransaction ret = new SubTransaction
            {
                Category = tempCategory,
                Memo = YnabCsvImport.MemoPartsRegex.Match(ynabTransaction.Memo).Groups["subTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
            };
            return ret;
        }

        /// <summary>
        /// This instance is a SubElement of the Parent (has to be a Transaction)
        /// </summary>
        [Write(false)]
        public override TitNoTransfer Parent
        {
            get { return _parent; }
            set
            {
                if (value is Transaction)
                {
                    _parent = value;
                    if(Id != -1) Update();
                    OnPropertyChanged();
                }
                else
                    throw new ArgumentException($"{nameof(Parent)} of a {nameof(SubTransaction)} has to be of Type {nameof(Transaction)}!");
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="parent">This instance is a SubElement of the parent</param>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubTransaction(Transaction parent = null, Category category = null, long sum = 0L, string memo = null) 
            : base(category, memo, sum)
        {
            ConstrDbLock = true;

            _parent = parent ?? _parent;

            ConstrDbLock = false;
        }

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="parentId">Id of the Parent</param>
        /// <param name="categoryId">Id of the Category</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The Sum of the SubElement</param>
        public SubTransaction(long id, long parentId, long categoryId, string memo, long sum) : base(id, parentId, categoryId, sum, memo)
        {
            ConstrDbLock = true;

            ConstrDbLock = false;
        }

        protected override void InsertToDb()
        {
            Database?.Insert(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
        }

        protected override void DeleteFromDb()
        {
            Database?.Delete(this);
        }
    }
}
