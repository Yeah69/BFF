using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
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
                Assert.True(payee.Id == id, $"{nameof(payee.Id)} not set right!");
                Assert.True(payee.Name == name, $"{nameof(payee.Name)} not set right!");
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Payee payee = new Payee();

                //Act

                //Assert
                Assert.True(payee.Id == -1L, $"Default {nameof(payee.Id)} not set right!");
                Assert.True(payee.Name == null, $"Default {nameof(payee.Name)} not set right!");
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
                Assert.True(payee.ToString() == name, $"{nameof(payee.ToString)}() does not work right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Payee payee = new Payee(1, "Someone");
                Mock<IBffOrm> ormMock = OrmMock();

                //Act
                payee.Insert(ormMock.Object);
                payee.Update(ormMock.Object);
                payee.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Payee>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Payee>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Payee>()), Times.Exactly(1));
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

        public static Mock<IBffOrm> OrmMock()
        {
            Mock<IBffOrm> ormMock = new Mock<IBffOrm>();
            ormMock.Setup(orm => orm.Insert(It.IsAny<Payee>())).Verifiable();
            ormMock.Setup(orm => orm.Update(It.IsAny<Payee>())).Verifiable();
            ormMock.Setup(orm => orm.Delete(It.IsAny<Payee>())).Verifiable();

            return ormMock;
        }
    }
}
