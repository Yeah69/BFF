using System;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    /// <summary>
    /// A SubElement of an Income
    /// </summary>
    public class SubIncome : SubTitBase
    {
        private TitNoTransfer _parent;

        /// <summary>
        /// This instance is a SubElement of the Parent (has to be an Income)
        /// </summary>
        [Write(false)]
        public override TitNoTransfer Parent
        {
            get { return _parent; }
            set
            {
                if (value is Income)
                {
                    _parent = value;
                    Update();
                    OnPropertyChanged();
                }
                else
                    throw new ArgumentException($"{nameof(Parent)} of a {nameof(SubIncome)} has to be of Type {nameof(Income)}!");
            }
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="parent">This instance is a SubElement of the parent</param>
        /// <param name="category">Category of the SubElement</param>
        /// <param name="sum">The Sum of the SubElement</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        public SubIncome(Income parent, Category category = null, string memo = null, long sum = 0L) 
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
        public SubIncome(long id, long parentId, long categoryId, string memo, long sum) : base(id, parentId, categoryId, sum, memo)
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
