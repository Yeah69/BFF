using System.Globalization;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IDbSetting : IDataModel
    {
        string CurrencyCultureName { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        string DateCultureName { get; set; }
        CultureInfo DateCulture { get; set; }
    }

    public class DbSetting : DataModel<IDbSetting>, IDbSetting
    {
        public string CurrencyCultureName
        {
            get => CurrencyCulture.Name;
            set
            {
                if(_currencyCulture.Equals(CultureInfo.GetCultureInfo(value))) return;
                _currencyCulture = CultureInfo.GetCultureInfo(value);
                UpdateAndNotify();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyCulture));
            }
        }

        private CultureInfo _currencyCulture;
        
        public CultureInfo CurrencyCulture
        {
            get => _currencyCulture;
            set
            {
                if(_currencyCulture.Equals(value)) return;
                _currencyCulture = value;
                UpdateAndNotify();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyCultureName));
            }
        }

        public string DateCultureName
        {
            get => DateCulture.Name;
            set
            {
                if (_dateCulture.Equals(CultureInfo.GetCultureInfo(value))) return;
                _dateCulture = CultureInfo.GetCultureInfo(value);
                UpdateAndNotify();
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateCulture));
            }
        }

        private CultureInfo _dateCulture;
        
        public CultureInfo DateCulture
        {
            get => _dateCulture;
            set
            {
                if (_dateCulture.Equals(value)) return;
                _dateCulture = value;
                UpdateAndNotify();
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateCultureName));
            }
        }

        public DbSetting(IRepository<IDbSetting> repository, long id) : base(repository, id)
        {
            _currencyCulture = CultureInfo.GetCultureInfo("de-DE");
            _dateCulture = CultureInfo.GetCultureInfo("de-DE");
        }
    }
}
