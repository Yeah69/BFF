using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
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
            _ = viewModel.Id;

            //Assert
            _ = mock.Received().Id;
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
    }
}