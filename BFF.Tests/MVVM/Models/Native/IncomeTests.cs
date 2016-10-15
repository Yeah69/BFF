﻿using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class IncomeTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> IncomeData => new[]
            {
                new object[] { 1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true },
                new object[] { -1, -1, DateTime.Today - TimeSpan.FromDays(69), 23, -1, "Long ago", 323L, false },
                new object[] { 11, 455, DateTime.Today + TimeSpan.FromDays(69), -1, 3, "Positive Income", -87978L, true }
            };

            [Theory, MemberData(nameof(IncomeData))]
            public void ConstructionTheory(long id, long accountId, DateTime date, long payeeId, long categoryId, string memo, long sum, bool cleared)
            {
                //Arrange
                Income income = new Income(id, accountId, date, payeeId, categoryId, memo, sum, cleared);

                //Act

                //Assert
                Assert.True(income.Id == id, $"{nameof(income.Id)} not set right!");
                Assert.True(income.AccountId == accountId, $"{nameof(income.AccountId)} not set right!");
                Assert.True(income.Date == date, $"{nameof(income.Date)} not set right!");
                Assert.True(income.PayeeId == payeeId, $"{nameof(income.PayeeId)} not set right!");
                Assert.True(income.CategoryId == categoryId, $"{nameof(income.CategoryId)} not set right!");
                Assert.True(income.Memo == memo, $"{nameof(income.Memo)} not set right!");
                Assert.True(income.Sum == sum, $"{nameof(income.Sum)} not set right!");
                Assert.True(income.Cleared == cleared, $"{nameof(income.Cleared)} not set right!");
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Income income = new Income(today);

                //Act

                //Assert
                Assert.True(income.Id == -1L, $"Default {nameof(income.Id)} not set right!");
                Assert.True(income.AccountId == -1L, $"Default {nameof(income.AccountId)} not set right!");
                Assert.True(income.Date == today, $"Default {nameof(income.Date)} not set right!");
                Assert.True(income.PayeeId == -1L, $"Default {nameof(income.PayeeId)} not set right!");
                Assert.True(income.CategoryId == -1L, $"Default {nameof(income.CategoryId)} not set right!");
                Assert.True(income.Memo == null, $"Default {nameof(income.Memo)} not set right!");
                Assert.True(income.Sum == 0L, $"Default {nameof(income.Sum)} not set right!");
                Assert.True(income.Cleared == false, $"Default {nameof(income.Cleared)} not set right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Income income = new Income(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);
                Mock<IBffOrm> ormMock = IBffOrmMock.BffOrmMock;

                //Act
                income.Insert(ormMock.Object);
                income.Update(ormMock.Object);
                income.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Income>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Income>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Income>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Income income = new Income(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => income.Insert(null));
                Assert.Throws<ArgumentNullException>(() => income.Update(null));
                Assert.Throws<ArgumentNullException>(() => income.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Income income = new Income(1, 1, DateTime.Today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.PropertyChanged(income, nameof(income.Id), () => income.Id = 69);
                Assert.PropertyChanged(income, nameof(income.AccountId), () => income.AccountId = 69);
                Assert.PropertyChanged(income, nameof(income.Date), () => income.Date = DateTime.Today - TimeSpan.FromDays(3));
                Assert.PropertyChanged(income, nameof(income.PayeeId), () => income.PayeeId = 69);
                Assert.PropertyChanged(income, nameof(income.CategoryId), () => income.CategoryId = 69);
                Assert.PropertyChanged(income, nameof(income.Memo), () => income.Memo = "Hangover?");
                Assert.PropertyChanged(income, nameof(income.Sum), () => income.Sum = 69L);
                Assert.PropertyChanged(income, nameof(income.Cleared), () => income.Cleared = false);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                Income income = new Income(1, 1, today, 2, 3, "Yeah, Party!", 6969L, true);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.Id), () => income.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.AccountId), () => income.AccountId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.Date), () => income.Date = today));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.PayeeId), () => income.PayeeId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.CategoryId), () => income.CategoryId = 3));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.Memo), () => income.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.Sum), () => income.Id = 6969L));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(income, nameof(income.Cleared), () => income.Cleared = true));
            }
        }
    }
}