using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// Someone to whom was payeed or who payeed himself
    /// </summary>
    public class Payee : CommonProperty
    {
        /// <summary>
        /// Representing string
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Initializing the object
        /// </summary>
        /// <param name="id">The objects Id</param>
        /// <param name="name">Name of the Payee</param>
        public Payee(long id = -1L, string name = null) : base(name)
        {
            ConstrDbLock = true;

            if (id > 0L) Id = id;

            ConstrDbLock = false;
        }

        protected override void InsertToDb()
        {
            Database?.CommonPropertyProvider.Add(this);
        }

        protected override void UpdateToDb()
        {
            Database?.Update(this);
        }

        protected override void DeleteFromDb()
        {
            Database?.CommonPropertyProvider.Remove(this);
        }
    }
}
