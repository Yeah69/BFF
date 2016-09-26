using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;
using Xunit;

namespace BFF.Tests.MVVM.Models.Native
{
    public class CategoryTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> CategoryData => new[]
            {
                new object[] { 1L, 2L , "Rent"},
                new object[] { 2L, 3L ,"Bribe"},
                new object[] { -1L, 1L ,"Food"}
            };

            [Theory, MemberData(nameof(CategoryData))]
            public void ConstructionTheory(long id, long parentId, string name)
            {
                //Arrange
                Category category = new Category(id, parentId, name);

                //Act

                //Assert
                Assert.True(category.Id == id, $"{nameof(category.Id)} not set right!");
                Assert.True(category.ParentId == parentId, $"{nameof(category.ParentId)} not set right!");
                Assert.True(category.Name == name, $"{nameof(category.Name)} not set right!");
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Category category = new Category();

                //Act

                //Assert
                Assert.True(category.Id == -1L, $"Default {nameof(category.Id)} not set right!");
                Assert.True(category.ParentId == null, $"{nameof(category.ParentId)} not set right!");
                Assert.True(category.Name == null, $"Default {nameof(category.Name)} not set right!");
            }
        }

        public class ToStringTest
        {
            public static IEnumerable<object[]> CategoryData => new[]
            {
                new object[] { 1L, 2L , "Rent"},
                new object[] { 2L, 3L ,"Bribe"},
                new object[] { -1L, 1L ,"Food"}
            };

            [Theory, MemberData(nameof(CategoryData))]
            public void ToStringTheory(long id, long parentId, string name)
            {
                //Arrange
                Category category = new Category(id, parentId, name);

                //Act

                //Assert
                Assert.True(category.ToString() == name, $"{nameof(category.ToString)}() does not work right!");
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Category category = new Category(-1, -1, "ACategory");
                Mock<IBffOrm> ormMock = OrmMock();

                //Act
                category.Insert(ormMock.Object);
                category.Update(ormMock.Object);
                category.Delete(ormMock.Object);

                //Assert
                ormMock.Verify(orm => orm.Insert(It.IsAny<Category>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Update(It.IsAny<Category>()), Times.Exactly(1));
                ormMock.Verify(orm => orm.Delete(It.IsAny<Category>()), Times.Exactly(1));
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                Category category = new Category(-1, -1, "ACategory");

                //Act
                Assert.Throws<ArgumentNullException>(() => category.Insert(null));
                Assert.Throws<ArgumentNullException>(() => category.Update(null));
                Assert.Throws<ArgumentNullException>(() => category.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                Category category = new Category(-1, -1, "ACategory");

                //Act + Assert
                Assert.PropertyChanged(category, nameof(category.Id), () => category.Id = 69);
                Assert.PropertyChanged(category, nameof(category.Name), () => category.Name = "AnotherCategory");
                Assert.PropertyChanged(category, nameof(category.ParentId), () => category.ParentId = 323);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                Category category = new Category(1, 6969, "ACategory");

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(category, nameof(category.Id), () => category.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(category, nameof(category.Name), () => category.Name = "ACategory"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(category, nameof(category.ParentId), () => category.ParentId = 6969));
            }
        }

        public static Mock<IBffOrm> OrmMock()
        {
            Mock<IBffOrm> ormMock = new Mock<IBffOrm>();
            ormMock.Setup(orm => orm.Insert(It.IsAny<Category>())).Verifiable();
            ormMock.Setup(orm => orm.Update(It.IsAny<Category>())).Verifiable();
            ormMock.Setup(orm => orm.Delete(It.IsAny<Category>())).Verifiable();

            return ormMock;
        }
    }
}
