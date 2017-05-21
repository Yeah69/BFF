using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    public abstract class DataModelViewModelTests<T> where T : DataModelViewModel
    {
        protected abstract (T, IDataModel) CreateDataModelViewModel(IBffOrm orm, long modelId);

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_NotInsertedModel_CallsInsertOnOrm()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, IDataModel modelMock) = CreateDataModelViewModel(ormFake, -1);
            
            //Act
            viewModel.Insert();

            //Assert
            modelMock.Received().Insert(ormFake);
        }

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_InsertedModel_CallsInsertOnOrm()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, IDataModel modelMock) = CreateDataModelViewModel(ormFake, 1);

            //Act
            viewModel.Insert();

            //Assert
            modelMock.DidNotReceive().Insert(ormFake);
        }

        public static IEnumerable<object[]> AtLeastOneNullCommonPropertyProvider
            => new []
            {
                new object [] {CommonPropertyProviderMoq.NullAccountViewModel},
                new object [] {CommonPropertyProviderMoq.NullCategoryViewModel},
                new object [] {CommonPropertyProviderMoq.NullPayeeViewModel}
            };

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public virtual void Insert_AtLeastOneNullCommonPropertyProvider_NotInserted_DoesntCallInsertOnOrm(ICommonPropertyProvider commonPropertyProvider)
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            ormFake.CommonPropertyProvider.Returns(info => commonPropertyProvider);
            (T viewModel, IDataModel modelMock) = CreateDataModelViewModel(ormFake, -1);

            //Act
            viewModel.Insert();

            //Assert
            modelMock.DidNotReceive().Insert(ormFake);
        }

        [Fact]
        public void Delete_InsertedModel_CallsDeleteOnOrm()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, IDataModel modelMock) = CreateDataModelViewModel(ormFake, 1);

            //Act
            viewModel.Delete();

            //Assert
            modelMock.Received().Delete(ormFake);
        }

        [Fact]
        public void Delete_NotInsertedModel_DoesntCallDeleteOnOrm()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, IDataModel modelMock) = CreateDataModelViewModel(BffOrmMoq.Naked, -1);

            //Act
            viewModel.Delete();

            //Assert
            modelMock.DidNotReceive().Delete(ormFake);
        }

        [Fact]
        public void ValidToInsert_AllNonNullCommonPropertyProvider_NotInsertedModel_True()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, _) = CreateDataModelViewModel(ormFake, -1);

            //Act
            bool result = viewModel.ValidToInsert();

            //Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(AtLeastOneNullCommonPropertyProvider))]
        public virtual void ValidToInsert_AtLeastOneNullCommonPropertyProvider_NotInserted_False(ICommonPropertyProvider commonPropertyProvider)
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            ormFake.CommonPropertyProvider.Returns(info => commonPropertyProvider);
            (T viewModel, _) = CreateDataModelViewModel(ormFake, -1);

            //Act
            bool result = viewModel.ValidToInsert();

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void IdGet_CallsModelIdGet()
        {
            //Arrange
            IBffOrm ormFake = BffOrmMoq.Naked;
            (T viewModel, IDataModel mock) = CreateDataModelViewModel(ormFake, 69);

            //Act
            _ = viewModel.Id;

            //Assert
            _ = mock.Received().Id;
        }
    }
}