using System;
using System.Globalization;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IDbSetting : IDataModelBase
    {
        string CurrencyCultrureName { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        string DateCultureName { get; set; }
        CultureInfo DateCulture { get; set; }
    }

    public class DbSetting : DataModelBase, IDbSetting
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
            }
        }

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Insert(this);
        }

        public override void Update(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Update(this);
        }

        public override void Delete(IBffOrm orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm));
            orm.Delete(this);
        }

        #endregion
    }
}
