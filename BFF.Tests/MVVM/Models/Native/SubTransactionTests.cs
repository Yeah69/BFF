using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class SubTransactionTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> SubTransactionData => new[]
            {
                new object[] { 1, 1, 2, "Yeah, Party!", -6969L },
                new object[] { -1, -1, 23, "Long ago", -323L },
                new object[] { 11, 455, -1, "Positive SubTransaction", 87978L }
            };

            [Theory, MemberData(nameof(SubTransactionData))]
            public void ConstructionTheory(long id, long parentId, long categoryId, string memo, long sum)
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction(id, parentId, categoryId, memo, sum);

                //Act

                //Assert
                Assert.Equal(id, subTransaction.Id);
                Assert.Equal(parentId, subTransaction.ParentId);
                Assert.Equal(categoryId, subTransaction.CategoryId);
                Assert.Equal(memo, subTransaction.Memo);
                Assert.Equal(sum, subTransaction.Sum);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction();

                //Act

                //Assert
                Assert.Equal(-1L, subTransaction.Id);
                Assert.Equal(-1L, subTransaction.ParentId);
                Assert.Equal(-1L, subTransaction.CategoryId);
                Assert.Equal(null, subTransaction.Memo);
                Assert.Equal(0L, subTransaction.Sum);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction(1, 1, 2, "Yeah, Party!", -6969L);
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                subTransaction.Insert(ormMock.Object);
                subTransaction.Update(ormMock.Object);
                subTransaction.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<SubTransaction>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<SubTransaction>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<SubTransaction>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => subTransaction.Insert(null));
                Assert.Throws<ArgumentNullException>(() => subTransaction.Update(null));
                Assert.Throws<ArgumentNullException>(() => subTransaction.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.PropertyChanged(subTransaction, nameof(subTransaction.Id), () => subTransaction.Id = 69);
                Assert.PropertyChanged(subTransaction, nameof(subTransaction.ParentId), () => subTransaction.ParentId = 69);
                Assert.PropertyChanged(subTransaction, nameof(subTransaction.CategoryId), () => subTransaction.CategoryId = 23);
                Assert.PropertyChanged(subTransaction, nameof(subTransaction.Memo), () => subTransaction.Memo = "Hangover?");
                Assert.PropertyChanged(subTransaction, nameof(subTransaction.Sum), () => subTransaction.Sum = 69L);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                SubTransaction subTransaction = new SubTransaction(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subTransaction, nameof(subTransaction.Id), () => subTransaction.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subTransaction, nameof(subTransaction.ParentId), () => subTransaction.ParentId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subTransaction, nameof(subTransaction.CategoryId), () => subTransaction.CategoryId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subTransaction, nameof(subTransaction.Memo), () => subTransaction.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subTransaction, nameof(subTransaction.Sum), () => subTransaction.Id = 6969L));
            }
        }
    }
}