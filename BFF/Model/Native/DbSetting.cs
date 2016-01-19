using System.Globalization;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class DbSetting : DataModelBase
    {
        public string CurrencyCultrureName { get; set; }

        [Write(false)]
        public CultureInfo CurrencyCulture
        {
            get
            {
                return CultureInfo.GetCultureInfo(CurrencyCultrureName);
            }
            set
            {
                CurrencyCultrureName = value.Name;
                Update();
            }
        }

        public string DateCultureName { get; set; }

        [Write(false)]
        public CultureInfo DateCulture
        {
            get
            {
                return CultureInfo.GetCultureInfo(DateCultureName);
            }
            set
            {
                DateCultureName = value.Name;
                Update();
            }
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
