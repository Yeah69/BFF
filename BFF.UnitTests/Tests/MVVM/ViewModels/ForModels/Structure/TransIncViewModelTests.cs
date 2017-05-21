using System;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TransIncViewModelTests<T> : TransIncBaseViewModelTests<T> where T : TransIncViewModel
    {
        protected abstract (T, ITransInc) TransIncViewModelFactory { get; }

        protected abstract ICategoryViewModel CategoryInitialValue { get; }
        protected abstract ICategoryViewModel CategoryDifferentValue { get; }

        [Fact]
        public void CategoryGet_CallsModelCategoryGet()
        {
            //Arrange
            (T viewModel, ITransInc mock) = TransIncViewModelFactory;

            //Act
            _ = viewModel.Category;

            //Assert
            _ = mock.Received().CategoryId;
        }

        [Fact]
        public void CategorySet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T transIncViewModel, _) = TransIncViewModelFactory;
            transIncViewModel.Category = CategoryInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncViewModel.Category = CategoryDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncViewModel, nameof(transIncViewModel.Category), shouldTriggerNotification);
        }

        [Fact]
        public void CategorySet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T transIncViewModel, _) = TransIncViewModelFactory;
            transIncViewModel.Category = CategoryInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncViewModel.Category = CategoryInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncViewModel, nameof(transIncViewModel.Category), shouldNotTriggerNotification);
        }

        [Fact]
        public void CategorySet_ChangedValue_CallsModelCategory()
        {
            //Arrange
            (T transIncViewModel, ITransInc mock) = TransIncViewModelFactory;
            transIncViewModel.Category = CategoryInitialValue;

            //Act
            transIncViewModel.Category = CategoryDifferentValue;

            //Assert
            mock.Received().CategoryId = CategoryDifferentValue.Id;
        }

        [Fact]
        public void CategorySet_SameValue_DoesNtCallModelCategory()
        {
            //Arrange
            (T transIncViewModel, ITransInc mock) = TransIncViewModelFactory;
            transIncViewModel.Category = CategoryInitialValue;

            //Act
            transIncViewModel.Category = CategoryInitialValue;

            //Assert
            mock.DidNotReceive().CategoryId = CategoryInitialValue.Id;
        }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void SumGet_CallsModelSumGet()
        {
            //Arrange
            (T viewModel, ITransInc mock) = TransIncViewModelFactory;

            //Act
            _ = viewModel.Sum;

            //Assert
            _ = mock.Received().Sum;
        }

        [Fact]
        public void SumSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T transIncViewModel, _) = TransIncViewModelFactory;
            transIncViewModel.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => transIncViewModel.Sum = SumDifferentValue;

            //Assert
            Assert.PropertyChanged(transIncViewModel, nameof(transIncViewModel.Sum), shouldTriggerNotification);
        }

        [Fact]
        public void SumSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T transIncViewModel, _) = TransIncViewModelFactory;
            transIncViewModel.Sum = SumInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => transIncViewModel.Sum = SumInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(transIncViewModel, nameof(transIncViewModel.Sum), shouldNotTriggerNotification);
        }

        [Fact]
        public void SumSet_ChangedValue_CallsModelSum()
        {
            //Arrange
            (T transIncViewModel, ITransInc mock) = TransIncViewModelFactory;
            transIncViewModel.Sum = SumInitialValue;

            //Act
            transIncViewModel.Sum = SumDifferentValue;

            //Assert
            mock.Received().Sum = SumDifferentValue;
        }

        [Fact]
        public void SumSet_SameValue_DoesNtCallModelSum()
        {
            //Arrange
            (T transIncViewModel, ITransInc mock) = TransIncViewModelFactory;
            transIncViewModel.Sum = SumInitialValue;

            //Act
            transIncViewModel.Sum = SumInitialValue;

            //Assert
            mock.DidNotReceive().Sum = SumInitialValue;
        }
    }
}