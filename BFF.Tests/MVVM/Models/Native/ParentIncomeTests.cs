using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class ParentIncomeTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> ParentIncomeData => new[]
            {
                new object[] { 1, 1, DateTime.Today, 2, "Yeah, Party!", true },
                new object[] { -1, -1, DateTime.Today - TimeSpan.FromDays(69), 23, "Long ago", false },
                new object[] { 11, 455, DateTime.Today + TimeSpan.FromDays(69), -1, "Positive ParentIncome", true }
            };

            [Theory, MemberData(nameof(ParentIncomeData))]
            public void ConstructionTheory(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
            {
                //Arrange
                ParentIncome parentIncome = new ParentIncome(id, accountId, date, payeeId, memo, cleared);

                //Act

                //Assert
                Assert.Equal(id, parentIncome.Id);
                Assert.Equal(accountId, parentIncome.AccountId);
                Assert.Equal(date, parentIncome.Date);
                Assert.Equal(payeeId, parentIncome.PayeeId);
                Assert.Equal(memo, parentIncome.Memo);
                Assert.Equal(cleared, parentIncome.Cleared);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                ParentIncome parentIncome = new ParentIncome(today);

                //Act

                //Assert
                Assert.Equal(-1L, parentIncome.Id);
                Assert.Equal(-1L, parentIncome.AccountId);
                Assert.Equal(today, parentIncome.Date);
                Assert.Equal(-1L, parentIncome.PayeeId);
                Assert.Equal(null, parentIncome.Memo);
                Assert.Equal(false, parentIncome.Cleared);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                ParentIncome parentIncome = new ParentIncome(1, 1, DateTime.Today, 2, "Yeah, Party!", true);
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                parentIncome.Insert(ormMock.Object);
                parentIncome.Update(ormMock.Object);
                parentIncome.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<ParentIncome>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<ParentIncome>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<ParentIncome>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                ParentIncome parentIncome = new ParentIncome(1, 1, DateTime.Today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => parentIncome.Insert(null));
                Assert.Throws<ArgumentNullException>(() => parentIncome.Update(null));
                Assert.Throws<ArgumentNullException>(() => parentIncome.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                ParentIncome parentIncome = new ParentIncome(1, 1, DateTime.Today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.Id), () => parentIncome.Id = 69);
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.AccountId), () => parentIncome.AccountId = 69);
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.Date), () => parentIncome.Date = DateTime.Today - TimeSpan.FromDays(3));
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.PayeeId), () => parentIncome.PayeeId = 69);
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.Memo), () => parentIncome.Memo = "Hangover?");
                Assert.PropertyChanged(parentIncome, nameof(parentIncome.Cleared), () => parentIncome.Cleared = false);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                ParentIncome parentIncome = new ParentIncome(1, 1, today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.Id), () => parentIncome.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.AccountId), () => parentIncome.AccountId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.Date), () => parentIncome.Date = today));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.PayeeId), () => parentIncome.PayeeId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.Memo), () => parentIncome.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentIncome, nameof(parentIncome.Cleared), () => parentIncome.Cleared = true));
            }
        }
    }
}