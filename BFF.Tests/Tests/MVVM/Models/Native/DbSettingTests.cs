using System;
using System.Globalization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using Moq;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class DbSettingTests
    {
        public class ConstructionTests
        {
            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DbSetting dbSetting = new DbSetting();

                //Act

                //Assert
                Assert.Equal(-1L, dbSetting.Id);
                Assert.Equal(CultureInfo.GetCultureInfo("de-DE"), dbSetting.CurrencyCulture);
                Assert.Equal(CultureInfo.GetCultureInfo("de-DE"), dbSetting.DateCulture);
                Assert.Equal(CultureInfo.GetCultureInfo("de-DE").Name, dbSetting.CurrencyCultureName);
                Assert.Equal(CultureInfo.GetCultureInfo("de-DE").Name, dbSetting.DateCultureName);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                DbSetting dbSetting = new DbSetting();
                Mock<IBffOrm> ormMock = BffOrmMoq.Mock;

                //Act
                dbSetting.Insert(ormMock.Object);
                dbSetting.Update(ormMock.Object);
                dbSetting.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<DbSetting>()), Times.Once);
                ormMock.Verify(orm => orm.Update(It.IsAny<DbSetting>()), Times.Once);
                ormMock.Verify(orm => orm.Delete(It.IsAny<DbSetting>()), Times.Once);
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                DbSetting dbSetting = new DbSetting();

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => dbSetting.Insert(null));
                Assert.Throws<ArgumentNullException>(() => dbSetting.Update(null));
                Assert.Throws<ArgumentNullException>(() => dbSetting.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                DbSetting dbSetting = new DbSetting();

                //Act + Assert
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.Id), () => dbSetting.Id = 69);
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), () => dbSetting.CurrencyCulture = CultureInfo.GetCultureInfo(69));
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), () => dbSetting.DateCulture = CultureInfo.GetCultureInfo(69));
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), () => dbSetting.CurrencyCulture = CultureInfo.GetCultureInfo(23));
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), () => dbSetting.DateCulture = CultureInfo.GetCultureInfo(23));
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), () => dbSetting.CurrencyCultureName = CultureInfo.GetCultureInfo(69).Name);
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), () => dbSetting.DateCultureName = CultureInfo.GetCultureInfo(69).Name);
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), () => dbSetting.CurrencyCultureName = CultureInfo.GetCultureInfo(23).Name);
                Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), () => dbSetting.DateCultureName = CultureInfo.GetCultureInfo(23).Name);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DbSetting dbSetting = new DbSetting();

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.Id), () => dbSetting.Id = -1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), () => dbSetting.CurrencyCulture = CultureInfo.GetCultureInfo("de-DE")));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), () => dbSetting.DateCulture = CultureInfo.GetCultureInfo("de-DE")));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), () => dbSetting.CurrencyCulture = CultureInfo.GetCultureInfo("de-DE")));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), () => dbSetting.DateCulture = CultureInfo.GetCultureInfo("de-DE")));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCulture), () => dbSetting.CurrencyCultureName = "de-DE"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCulture), () => dbSetting.DateCultureName = "de-DE"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.CurrencyCultureName), () => dbSetting.CurrencyCultureName = "de-DE"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(dbSetting, nameof(dbSetting.DateCultureName), () => dbSetting.DateCultureName = "de-DE"));
            }
        }
    }
}