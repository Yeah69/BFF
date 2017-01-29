using System;
using System.Globalization;
using BFF.MVVM.Models.Native;
using BFF.Tests.Helper;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class DbSettingTests : DataModelBaseTests<DbSetting>
    {
        protected override DbSetting DataModelBaseFactory => new DbSetting();
        protected override long IdInitialValue => -1L;
        protected override long IdDifferentValue => 69;

        DbSetting DbSettingFactory => new DbSetting();

        private long IdDefaultValue => -1;
        private CultureInfo CurrencyCultureDefaultValue => CultureInfo.GetCultureInfo("de-DE");
        private CultureInfo DateCultureDefaultValue => CultureInfo.GetCultureInfo("de-DE");
        private string CurrencyCultureNameDefaultValue => CultureInfo.GetCultureInfo("de-DE").Name;
        private string DateCultureNameDefaultValue => CultureInfo.GetCultureInfo("de-DE").Name;

        [Fact]
        public void Ctor_Nothing_HasDefaultId()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;

            //Act


            //Assert
            Assert.Equal(IdDefaultValue, dbSetting.Id);
        }

        [Fact]
        public void Ctor_Nothing_HasDefaultCurrencyCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;

            //Act


            //Assert
            Assert.Equal(CurrencyCultureDefaultValue, dbSetting.CurrencyCulture);
        }

        [Fact]
        public void Ctor_Nothing_HasDefaultCurrencyCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;

            //Act


            //Assert
            Assert.Equal(CurrencyCultureNameDefaultValue, dbSetting.CurrencyCultureName);
        }

        [Fact]
        public void Ctor_Nothing_HasDefaultDateCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;

            //Act


            //Assert
            Assert.Equal(DateCultureDefaultValue, dbSetting.DateCulture);
        }

        [Fact]
        public void Ctor_Nothing_HasDefaultDateCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;

            //Act


            //Assert
            Assert.Equal(DateCultureNameDefaultValue, dbSetting.DateCultureName);
        }

        private CultureInfo CurrencyCultureInitialValue => CultureInfo.GetCultureInfo("hu-HU");
        private CultureInfo CurrencyCultureDifferentValue => CultureInfo.GetCultureInfo("en-US");

        private string CurrencyCultureNameInitialValue => "hu-HU";
        private string CurrencyCultureNameDifferentValue => "en-US";

        [Fact]
        public void CurrencyCulture_ChangeValue_TriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.CurrencyCulture = CurrencyCultureDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), shouldTriggerNotification);
        }

        [Fact]
        public void CurrencyCulture_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), shouldNotTriggerNotification);
        }

        [Fact]
        public void CurrencyCulture_ChangeValue_TriggersNotificationOfCurrencyCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.CurrencyCulture = CurrencyCultureDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), shouldTriggerNotification);
        }

        [Fact]
        public void CurrencyCulture_SameValue_DoesntTriggersNotificationOfCurrencyCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.CurrencyCulture = CurrencyCultureInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), shouldNotTriggerNotification);
        }

        [Fact]
        public void CurrencyCultureName_ChangeValue_TriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.CurrencyCultureName = CurrencyCultureNameDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), shouldTriggerNotification);
        }

        [Fact]
        public void CurrencyCultureName_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), shouldNotTriggerNotification);
        }

        [Fact]
        public void CurrencyCultureName_ChangeValue_TriggersNotificationOfCurrencyCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.CurrencyCultureName = CurrencyCultureNameDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), shouldTriggerNotification);
        }

        [Fact]
        public void CurrencyCultureName_SameValue_DoesntTriggersNotificationOfCurrencyCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.CurrencyCultureName = CurrencyCultureNameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), shouldNotTriggerNotification);
        }

        private CultureInfo DateCultureInitialValue => CultureInfo.GetCultureInfo("hu-HU");
        private CultureInfo DateCultureDifferentValue => CultureInfo.GetCultureInfo("en-US");

        private string DateCultureNameInitialValue => "hu-HU";
        private string DateCultureNameDifferentValue => "en-US";

        [Fact]
        public void DateCulture_ChangeValue_TriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCulture = DateCultureInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.DateCulture = DateCultureDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), shouldTriggerNotification);
        }

        [Fact]
        public void DateCulture_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCulture = DateCultureInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.DateCulture = DateCultureInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.DateCulture), shouldNotTriggerNotification);
        }

        [Fact]
        public void DateCulture_ChangeValue_TriggersNotificationOfDateCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCulture = DateCultureInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.DateCulture = DateCultureDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), shouldTriggerNotification);
        }

        [Fact]
        public void DateCulture_SameValue_DoesntTriggersNotificationOfDateCultureName()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCulture = DateCultureInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.DateCulture = DateCultureInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), shouldNotTriggerNotification);
        }

        [Fact]
        public void DateCultureName_ChangeValue_TriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.DateCultureName = DateCultureNameDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), shouldTriggerNotification);
        }

        [Fact]
        public void DateCultureName_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), shouldNotTriggerNotification);
        }

        [Fact]
        public void DateCultureName_ChangeValue_TriggersNotificationOfDateCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Act
            Action shouldTriggerNotification = () => dbSetting.DateCultureName = DateCultureNameDifferentValue;

            //Assert
            Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), shouldTriggerNotification);
        }

        [Fact]
        public void DateCultureName_SameValue_DoesntTriggersNotificationOfDateCulture()
        {
            //Arrange
            DbSetting dbSetting = DbSettingFactory;
            dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => dbSetting.DateCultureName = DateCultureNameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(dbSetting, nameof(dbSetting.DateCulture), shouldNotTriggerNotification);
        }
    }
}