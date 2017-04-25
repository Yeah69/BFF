﻿using System;
using System.Globalization;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IDbSetting : IDataModelBase
    {
        string CurrencyCultureName { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        string DateCultureName { get; set; }
        CultureInfo DateCulture { get; set; }
    }

    public class DbSetting : DataModel, IDbSetting
    {
        public string CurrencyCultureName
        {
            get { return CurrencyCulture.Name; }
            set
            {
                if(_currencyCulture.Equals(CultureInfo.GetCultureInfo(value))) return;
                _currencyCulture = CultureInfo.GetCultureInfo(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyCulture));
            }
        }

        private CultureInfo _currencyCulture;

        [Write(false)]
        public CultureInfo CurrencyCulture
        {
            get
            {
                return _currencyCulture;
            }
            set
            {
                if(_currencyCulture.Equals(value)) return;
                _currencyCulture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrencyCultureName));
            }
        }

        public string DateCultureName
        {
            get { return DateCulture.Name; }
            set
            {
                if (_dateCulture.Equals(CultureInfo.GetCultureInfo(value))) return;
                _dateCulture = CultureInfo.GetCultureInfo(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateCulture));
            }
        }

        private CultureInfo _dateCulture;

        [Write(false)]
        public CultureInfo DateCulture
        {
            get
            {
                return _dateCulture;
            }
            set
            {
                if (_dateCulture.Equals(value)) return;
                _dateCulture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateCultureName));
            }
        }

        public DbSetting()
        {
            _currencyCulture = CultureInfo.GetCultureInfo("de-DE");
            _dateCulture = CultureInfo.GetCultureInfo("de-DE");
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
