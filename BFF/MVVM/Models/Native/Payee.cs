using System.Collections.Generic;
using System.Linq;
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

        private static readonly Dictionary<string, Payee> Cache = new Dictionary<string, Payee>();

        public static Payee GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            Payee payee = new Payee { Name = name };
            Cache.Add(name, payee);
            return payee;
        }

        public static List<Payee> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static void ClearCache()
        {
            Cache.Clear();
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
