using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.DB;
using BFF.Tests.Mocks.MVVM.Models.Native;
using BFF.Tests.Mocks.MVVM.ViewModels.ForModels;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels
{
    public static class CategoryViewModelTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> CategoryViewModelData =
                CategoryMoq.Mocks.Select(am => new object[] {am});

            [Theory, MemberData(nameof(CategoryViewModelData))]
            public void ConstructionTheory(ICategory category)
            {
                //Arrange
                CategoryViewModel categoryViewModel = new CategoryViewModel(category, BffOrmMoq.Mock);

                //Act

                //Assert
                Assert.Equal(category.Id, categoryViewModel.Id);
                Assert.Equal(category.Name, categoryViewModel.Name);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudInsertFact()
            {//todo: refactor so the whole list of CategoryMoq.NotInserted is checked.
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                CategoryViewModel insertCategory = new CategoryViewModel(CategoryMoq.NotInserted[0],
                                                                         BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                insertCategory.Insert();

                //Assert
                commonPropertyProviderMock.Received().Add(Arg.Any<ICategory>());
            }

            [Fact]
            public void CrudUpdateFact()
            {
                //Arrange
                ICategory mock = CategoryMoq.Mocks[0];
                CategoryViewModel updateCategory = new CategoryViewModel(mock, BffOrmMoq.Mock);

                //Act
                updateCategory.Name = "asdf";

                //Assert
                mock.Received().Update(Arg.Any<IBffOrm>());
            }
            [Fact]
            public void CrudDeleteFact()
            {
                //Arrange
                ICategory mock = CategoryMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.Mock;
                CategoryViewModel deleteCategory = new CategoryViewModel(mock, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act
                deleteCategory.Delete();

                //Assert
                commonPropertyProviderMock.Received().Remove(Arg.Any<ICategory>());
            }
        }

        public class PropertyTests
        {

            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                ICategory mock = CategoryMoq.Mocks[0];
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(summaryAccountViewModelMock:
                                                         SummaryAccountViewModelMoq.Mock);
                CategoryViewModel categoryViewModel = new CategoryViewModel(mock, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act + Assert
                Assert.PropertyChanged(categoryViewModel, nameof(categoryViewModel.Name), () => categoryViewModel.Name = "AnotherCategoryViewModel");
                Assert.PropertyChanged(categoryViewModel, nameof(categoryViewModel.Parent), () => categoryViewModel.Parent = commonPropertyProviderMock.ParentCategoryViewModels[1]);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                ICategory mock = CategoryMoq.Mocks[0];
                CategoryViewModel categoryViewModel = new CategoryViewModel(mock, BffOrmMoq.Mock);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(categoryViewModel, nameof(categoryViewModel.Name), () => categoryViewModel.Name = categoryViewModel.Name));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(categoryViewModel, nameof(categoryViewModel.Parent), () => categoryViewModel.Parent = categoryViewModel.Parent));
            }
        }

        public class ValidToInsertTests
        {
            public static IEnumerable<object[]> ValidToInsert =
                CategoryMoq.NotInserted.Select(am => new object[] {am});

            public static IEnumerable<object[]> NotValidToInsert =
                CategoryMoq.NotValidToInsert.Select(am => new object[] {am});

            [Theory, MemberData(nameof(ValidToInsert))]
            public void ValidToInsertTest(ICategory category)
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock = CommonPropertyProviderMoq.CreateMock();
                CategoryViewModel categoryViewModel = new CategoryViewModel(category,
                                                                            BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act

                //Assert
                Assert.True(categoryViewModel.ValidToInsert());
            }

            [Theory, MemberData(nameof(NotValidToInsert))]
            public void NotValidToInsertTest(ICategory category)
            {
                //Arrange
                ICommonPropertyProvider commonPropertyProviderMock =
                    CommonPropertyProviderMoq.CreateMock(categoryViewModelMocks: new List<ICategoryViewModel>());
                var categoryViewModel = new CategoryViewModel(category, BffOrmMoq.CreateMock(commonPropertyProviderMock));

                //Act

                //Assert
                Assert.False(categoryViewModel.ValidToInsert());
            }
        }
    }
}