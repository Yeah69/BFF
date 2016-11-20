using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using Moq;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class ParentTransactionTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> ParentTransactionData => new[]
            {
                new object[] { 1, 1, DateTime.Today, 2, "Yeah, Party!", true },
                new object[] { -1, -1, DateTime.Today - TimeSpan.FromDays(69), 23, "Long ago", false },
                new object[] { 11, 455, DateTime.Today + TimeSpan.FromDays(69), -1, "Positive ParentTransaction", true }
            };

            [Theory, MemberData(nameof(ParentTransactionData))]
            public void ConstructionTheory(long id, long accountId, DateTime date, long payeeId, string memo, bool cleared)
            {
                //Arrange
                ParentTransaction parentTransaction = new ParentTransaction(id, accountId, date, payeeId, memo, cleared);

                //Act

                //Assert
                Assert.Equal(id, parentTransaction.Id);
                Assert.Equal(accountId, parentTransaction.AccountId);
                Assert.Equal(date, parentTransaction.Date);
                Assert.Equal(payeeId, parentTransaction.PayeeId);
                Assert.Equal(memo, parentTransaction.Memo);
                Assert.Equal(cleared, parentTransaction.Cleared);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                ParentTransaction parentTransaction = new ParentTransaction(today);

                //Act

                //Assert
                Assert.Equal(-1L, parentTransaction.Id);
                Assert.Equal(-1L, parentTransaction.AccountId);
                Assert.Equal(today, parentTransaction.Date);
                Assert.Equal(-1L, parentTransaction.PayeeId);
                Assert.Equal(null, parentTransaction.Memo);
                Assert.Equal(false, parentTransaction.Cleared);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                ParentTransaction parentTransaction = new ParentTransaction(1, 1, DateTime.Today, 2, "Yeah, Party!", true);
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                 parentTransaction.Insert(ormMock.Object);
                 parentTransaction.Update(ormMock.Object);
                 parentTransaction.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<ParentTransaction>()), Times.Once);
                ormMock.Verify(orm => orm.Update(It.IsAny<ParentTransaction>()), Times.Once);
                ormMock.Verify(orm => orm.Delete(It.IsAny<ParentTransaction>()), Times.Once);
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                ParentTransaction parentTransaction = new ParentTransaction(1, 1, DateTime.Today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => parentTransaction.Insert(null));
                Assert.Throws<ArgumentNullException>(() => parentTransaction.Update(null));
                Assert.Throws<ArgumentNullException>(() => parentTransaction.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                ParentTransaction parentTransaction = new ParentTransaction(1, 1, DateTime.Today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Id), () => parentTransaction.Id = 69);
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.AccountId), () => parentTransaction.AccountId = 69);
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Date), () => parentTransaction.Date = DateTime.Today - TimeSpan.FromDays(3));
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.PayeeId), () => parentTransaction.PayeeId = 69);
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Memo), () => parentTransaction.Memo = "Hangover?");
                Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Cleared), () => parentTransaction.Cleared = false);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                DateTime today = DateTime.Today;
                ParentTransaction parentTransaction = new ParentTransaction(1, 1, today, 2, "Yeah, Party!", true);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Id), () => parentTransaction.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.AccountId), () => parentTransaction.AccountId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Date), () => parentTransaction.Date = today));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.PayeeId), () => parentTransaction.PayeeId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Memo), () => parentTransaction.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(parentTransaction, nameof(parentTransaction.Cleared), () => parentTransaction.Cleared = true));
            }
        }

        public class SubElementTests
        {
            [Fact]
            public void SubElementFact()
            {
                //Arrange
                IList<Mock<ISubTransaction>> subTransactionMocks = SubTransactionMoq.Mocks;
                Mock<IBffOrm> ormMock = BffOrmMoq.CreateMock(subTransationMocks: subTransactionMocks);
                DateTime today = DateTime.Today;
                ParentTransaction parentTransaction = new ParentTransaction(1, 1, today, 2, "Yeah, Party!", true);
                
                //Act
                IList<ISubTransInc> subTransactions = parentTransaction.GetSubTransInc(ormMock.Object).ToList();

                //Assert
                foreach(ISubTransaction subTransaction in subTransactionMocks.Select(stm => stm.Object).Where(st => st.ParentId == parentTransaction.Id))
                {
                    Assert.Contains(subTransaction, subTransactions);
                }
            }
        }
    }
}