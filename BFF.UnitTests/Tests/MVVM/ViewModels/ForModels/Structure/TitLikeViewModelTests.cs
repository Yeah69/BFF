using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded")]
    public abstract class TitLikeViewModelTests<T> : DataModelViewModelTests<T> where T : TitLikeViewModel
    {
        protected abstract (T, ITitLike) TitLikeViewModelFactory { get; }

        protected abstract string MemoInitialValue { get; }
        protected abstract string MemoDifferentValue { get; }

        [Fact]
        public void MemoGet_CallsModelMemoGet()
        {
            //Arrange
            (T viewModel, ITitLike mock) = TitLikeViewModelFactory;

            //Act
            _ = viewModel.Memo;

            //Assert
            _ = mock.Received().Memo;
        }

        [Fact]
        public void MemoSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T titLikeViewModel, _) = TitLikeViewModelFactory;
            titLikeViewModel.Memo = MemoInitialValue;

            //Act
            Action shouldTriggerNotification = () => titLikeViewModel.Memo = MemoDifferentValue;

            //Assert
            Assert.PropertyChanged(titLikeViewModel, nameof(titLikeViewModel.Memo), shouldTriggerNotification);
        }

        [Fact]
        public void MemoSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T titLikeViewModel, _) = TitLikeViewModelFactory;
            titLikeViewModel.Memo = MemoInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => titLikeViewModel.Memo = MemoInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titLikeViewModel, nameof(titLikeViewModel.Memo), shouldNotTriggerNotification);
        }

        [Fact]
        public void MemoSet_ChangedValue_CallsModelMemo()
        {
            //Arrange
            (T titLikeViewModel, ITitLike mock) = TitLikeViewModelFactory;
            titLikeViewModel.Memo = MemoInitialValue;

            //Act
            titLikeViewModel.Memo = MemoDifferentValue;

            //Assert
            mock.Received().Memo = MemoDifferentValue;
        }

        [Fact]
        public void MemoSet_SameValue_DoesNtCallModelMemo()
        {
            //Arrange
            (T titLikeViewModel, ITitLike mock) = TitLikeViewModelFactory;
            titLikeViewModel.Memo = MemoInitialValue;

            //Act
            titLikeViewModel.Memo = MemoInitialValue;

            //Assert
            mock.DidNotReceive().Memo = MemoInitialValue;
        }

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_NotInsertedModel_CallsInsertOnOrm()
        {
            //Arrange
            var (viewModel, modelMock, ormFake) = CreateDataModelViewModel(-1);

            //Act
            viewModel.Insert();

            //Assert
            modelMock.Received().Insert(ormFake);
        }

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_InsertedModel_DoesNtCallInsertOnOrm()
        {
            //Arrange
            var (viewModel, modelMock, ormFake) = CreateDataModelViewModel(1);

            //Act
            viewModel.Insert();

            //Assert
            modelMock.DidNotReceive().Insert(ormFake);
        }

        public static IEnumerable<object[]> AtLeastOneNullCommonPropertyProvider
            => new[]
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
            var (viewModel, modelMock, ormFake) = CreateDataModelViewModel(-1, commonPropertyProvider);

            //Act
            viewModel.Insert();

            //Assert
            modelMock.DidNotReceive().Insert(ormFake);
        }

        [Fact]
        public void Delete_InsertedModel_CallsDeleteOnOrm()
        {
            //Arrange
            var (viewModel, modelMock, ormFake) = CreateDataModelViewModel(1);

            //Act
            viewModel.Delete();

            //Assert
            modelMock.Received().Delete(ormFake);
        }

        [Fact]
        public void Delete_NotInsertedModel_DoesntCallDeleteOnOrm()
        {
            //Arrange
            var (viewModel, modelMock, ormFake) = CreateDataModelViewModel(-1);

            //Act
            viewModel.Delete();

            //Assert
            modelMock.DidNotReceive().Delete(ormFake);
        }

        [Fact]
        public void ValidToInsert_AllNonNullCommonPropertyProvider_NotInsertedModel_True()
        {
            //Arrange
            var (viewModel, _, _) = CreateDataModelViewModel(-1);

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
            var (viewModel, _, _) = CreateDataModelViewModel(-1, commonPropertyProvider);

            //Act
            bool result = viewModel.ValidToInsert();

            //Assert
            Assert.False(result);
        }
    }
}