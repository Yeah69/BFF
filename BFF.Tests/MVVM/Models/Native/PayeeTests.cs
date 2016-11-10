using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.DB.Mock;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class PayeeTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> PayeeData => new[]
            {
                new object[] { 1L, "Mama" },
                new object[] { 2L, "Mafia" },
                new object[] { -1L, "Girlfriend" }
            };

            [Theory, MemberData(nameof(PayeeData))]
            public void ConstructionTheory(long id, string name)
            {
                //Arrange
                Payee payee = new Payee(id, name);

                //Act

                //Assert
                Assert.Equal(id, payee.Id);
                Assert.Equal(name, payee.Name);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Payee payee = new Payee();

                //Act

                //Assert
                Assert.Equal(-1L, payee.Id);
                Assert.Equal(null, payee.Name);
            }
        }

        public class ToStringTest
        {
            public static IEnumerable<object[]> PayeeData => new[]
            {
                new object[] { 1L, "Mama" },
                new object[] { 2L, "Mafia" },
                new object[] { -1L, "Girlfriend" }
            };

            [Theory, MemberData(nameof(PayeeData))]
            public void ToStringTheory(long id, string name)
            {
                //Arrange
                Payee payee = new Payee(id, name);

                //Act

                //Assert
                Assert.Equal(name, payee.ToString());
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Payee payee = new Payee(1, "Someone");
                Mock<IBffOrm> ormMock = BffOrmMoq.BffOrmMock;

                //Act
                payee.Insert(ormMock.Object);
                payee.Update(ormMock.Object);
                payee.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Payee>()), Times.Once);
                ormMock.Verify(orm => orm.Update(It.IsAny<Payee>()), Times.Once);
                ormMock.Verify(orm => orm.Delete(It.IsAny<Payee>()), Times.Once);
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Payee payee = new Payee(1, "Someone");

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => payee.Insert(null));
                Assert.Throws<ArgumentNullException>(() => payee.Update(null));
                Assert.Throws<ArgumentNullException>(() => payee.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Payee payee = new Payee(1, "Someone");

                //Act + Assert
                Assert.PropertyChanged(payee, nameof(payee.Id), () => payee.Id = 69);
                Assert.PropertyChanged(payee, nameof(payee.Name), () => payee.Name = "AnotherPerson");
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                Payee payee = new Payee(1, "Someone");

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(payee, nameof(payee.Id), () => payee.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(payee, nameof(payee.Name), () => payee.Name = "Someone"));
            }
        }
    }
}
