using System.Globalization;
using BFF.DB.SQLite;
using BFF.Helper;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class DbSetting : DataModelBase
    {
        public long Id { get; set; } = 1;

        public string CurrencyCultrureName { get; set; }

        [Write(false)]
        public CultureInfo CurrencyCulture
        {
            get
            {
                Output.CurrencyCulture = CultureInfo.GetCultureInfo(CurrencyCultrureName);
                return Output.CurrencyCulture;
            }
            set
            {
                CurrencyCultrureName = value.Name;
                SqLiteHelper.SetDbSetting(this);
                Output.CurrencyCulture = value;
            }
        }
    }
}
