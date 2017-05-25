using System;
using System.Diagnostics.CodeAnalysis;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded")]
    public abstract class TitBaseViewModelTests<T> : TitLikeViewModelTests<T> where T : TitBaseViewModel
    {
        protected abstract (T, ITitBase) TitBaseViewModelFactory { get; }

        protected abstract bool ClearedInitialValue { get; }
        protected abstract bool ClearedDifferentValue { get; }

        [Fact]
        public void ClearedGet_CallsModelClearedGet()
        {
            //Arrange
            (T viewModel, ITitBase mock) = TitBaseViewModelFactory;

            //Act
            _ = viewModel.Cleared;

            //Assert
            _ = mock.Received().Cleared;
        }

        [Fact]
        public void ClearedSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T titBaseViewModel, _) = TitBaseViewModelFactory;
            titBaseViewModel.Cleared = ClearedInitialValue;

            //Act
            Action shouldTriggerNotification = () => titBaseViewModel.Cleared = ClearedDifferentValue;

            //Assert
            Assert.PropertyChanged(titBaseViewModel, nameof(titBaseViewModel.Cleared), shouldTriggerNotification);
        }

        [Fact]
        public void ClearedSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T titBaseViewModel, _) = TitBaseViewModelFactory;
            titBaseViewModel.Cleared = ClearedInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => titBaseViewModel.Cleared = ClearedInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titBaseViewModel, nameof(titBaseViewModel.Cleared), shouldNotTriggerNotification);
        }

        [Fact]
        public void ClearedSet_ChangedValue_CallsModelCleared()
        {
            //Arrange
            (T titBaseViewModel, ITitBase mock) = TitBaseViewModelFactory;
            titBaseViewModel.Cleared = ClearedInitialValue;

            //Act
            titBaseViewModel.Cleared = ClearedDifferentValue;

            //Assert
            mock.Received().Cleared = ClearedDifferentValue;
        }

        [Fact]
        public void ClearedSet_SameValue_DoesNtCallModelCleared()
        {
            //Arrange
            (T titBaseViewModel, ITitBase mock) = TitBaseViewModelFactory;
            titBaseViewModel.Cleared = ClearedInitialValue;

            //Act
            titBaseViewModel.Cleared = ClearedInitialValue;

            //Assert
            mock.DidNotReceive().Cleared = ClearedInitialValue;
        }

        protected abstract DateTime DateInitialValue { get; }
        protected abstract DateTime DateDifferentValue { get; }

        [Fact]
        public void DateGet_CallsModelDateGet()
        {
            //Arrange
            (T viewModel, ITitBase mock) = TitBaseViewModelFactory;

            //Act
            _ = viewModel.Date;

            //Assert
            _ = mock.Received().Date;
        }

        [Fact]
        public void DateSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T titBaseViewModel, _) = TitBaseViewModelFactory;
            titBaseViewModel.Date = DateInitialValue;

            //Act
            Action shouldTriggerNotification = () => titBaseViewModel.Date = DateDifferentValue;

            //Assert
            Assert.PropertyChanged(titBaseViewModel, nameof(titBaseViewModel.Date), shouldTriggerNotification);
        }

        [Fact]
        public void DateSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T titBaseViewModel, _) = TitBaseViewModelFactory;
            titBaseViewModel.Date = DateInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => titBaseViewModel.Date = DateInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titBaseViewModel, nameof(titBaseViewModel.Date), shouldNotTriggerNotification);
        }

        [Fact]
        public void DateSet_ChangedValue_CallsModelDate()
        {
            //Arrange
            (T titBaseViewModel, ITitBase mock) = TitBaseViewModelFactory;
            titBaseViewModel.Date = DateInitialValue;

            //Act
            titBaseViewModel.Date = DateDifferentValue;

            //Assert
            mock.Received().Date = DateDifferentValue;
        }

        [Fact]
        public void DateSet_SameValue_DoesNtCallModelDate()
        {
            //Arrange
            (T titBaseViewModel, ITitBase mock) = TitBaseViewModelFactory;
            titBaseViewModel.Date = DateInitialValue;

            //Act
            titBaseViewModel.Date = DateInitialValue;

            //Assert
            mock.DidNotReceive().Date = DateInitialValue;
        }
    }
}