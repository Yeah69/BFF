using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public static class CategoryTests
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
                Assert.Equal(id, category.Id);
                Assert.Equal(parentId, category.ParentId);
                Assert.Equal(name, category.Name);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                Category category = new Category();

                //Act

                //Assert
                Assert.Equal(-1L, category.Id);
                Assert.Equal(null, category.ParentId);
                Assert.Equal(null, category.Name);
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
                Assert.Equal(name, category.ToString());
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                Category category = new Category(-1, -1, "ACategory");
                IBffOrm ormMock = BffOrmMoq.Mock;

                //Act
                category.Insert(ormMock);
                category.Update(ormMock);
                category.Delete(ormMock);

                //Assert
                ormMock.Received().Insert(Arg.Any<Category>());
                ormMock.Received().Update(Arg.Any<Category>());
                ormMock.Received().Delete(Arg.Any<Category>());
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
    }
}
