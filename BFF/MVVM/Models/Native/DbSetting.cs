using System.Globalization;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IDbSetting : ICrudBase
    {
        string CurrencyCultrureName { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        string DateCultureName { get; set; }
        CultureInfo DateCulture { get; set; }
    }

    public class DbSetting : CrudBase, IDbSetting
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
                if(Id != -1) Update();
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
                if(Id != -1) Update();
            }
        }
        
        protected override void InsertToDb()
        {
            Database?.Insert(this);
        }

        public override bool ValidToInsert()
        {
            return true;
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
