﻿using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading;
using BFF.DB;
using BFF.Model.Native;
using BFF.Properties;

namespace BFF.Helper
{
    internal class BffCultureProvider : IBffCultureProvider
    {
        private readonly IBffOrm _orm;
        private CultureInfo _customCulture;
        private CultureInfo _languageCulture = CultureInfo.GetCultureInfo("en-US");
        private CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("de-DE");
        private CultureInfo _dateCulture = CultureInfo.GetCultureInfo("de-DE");

        public CultureInfo LanguageCulture
        {
            get { return _languageCulture; }
            set
            {
                _languageCulture = value;
                createAndApplyCustomCulture();
                Settings.Default.Localization_Language = value.Name;
                Settings.Default.Save();
            }
        }

        public CultureInfo CurrencyCulture
        {
            get { return _currencyCulture; }
            set
            {
                _currencyCulture = value;
                createAndApplyCustomCulture();
                if (File.Exists(_orm.DbPath))
                {
                    DbSetting dbSetting = _orm.Get<DbSetting>(1);
                    dbSetting.CurrencyCulture = CurrencyCulture;
                }
            }
        }

        public CultureInfo DateCulture
        {
            get { return _dateCulture; }
            set
            {
                _dateCulture = value;
                createAndApplyCustomCulture();
                if (File.Exists(_orm.DbPath))
                {
                    DbSetting dbSetting = _orm.Get<DbSetting>(1);
                    dbSetting.DateCulture = DateCulture;
                }
            }
        }

        public BffCultureProvider(IBffOrm orm)
        {
            _orm = orm;
            _orm.DbPathChanged += (sender, args) => applyNewDatabase();
            LanguageCulture = CultureInfo.GetCultureInfo(Settings.Default.Localization_Language);
            if (File.Exists(_orm.DbPath))
            {
                applyNewDatabase();
            }
            else
            {
                createAndApplyCustomCulture();
            }
        }

        private void createAndApplyCustomCulture()
        {
            _customCulture = CultureInfo.CreateSpecificCulture(LanguageCulture.Name);
            _customCulture.NumberFormat = CurrencyCulture.NumberFormat;
            _customCulture.DateTimeFormat = DateCulture.DateTimeFormat;
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = _customCulture;
            Thread.CurrentThread.CurrentCulture = _customCulture;
            Thread.CurrentThread.CurrentUICulture = _customCulture;
        }

        private void applyNewDatabase()
        {
            if (File.Exists(_orm.DbPath))
            {
                DbSetting dbSetting = _orm.Get<DbSetting>(1);
                CurrencyCulture = dbSetting.CurrencyCulture;
                DateCulture = dbSetting.DateCulture;
                createAndApplyCustomCulture();
            }
        }
    }
}
