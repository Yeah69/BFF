using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.Tests.Helper;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    public abstract class SubTransIncViewModelTests<T> : TitLikeViewModelTests<T> where T : SubTransIncViewModel
    {
        protected abstract (T, ISubTransInc) SubTransIncViewModelFactory { get; }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void SumGet_CallsModelSumGet()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;

            //Act
            _ = viewModel.Sum;

            //Assert
            _ = mock.Received().Sum;
        }

        [Fact]
        public void SumSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T viewModel, _) = SubTransIncViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.Sum = SumDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.Sum), shouldTriggerNotification);
        }

        [Fact]
        public void SumSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T viewModel, _) = SubTransIncViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.Sum = SumInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.Sum), shouldNotTriggerNotification);
        }

        [Fact]
        public void SumSet_ChangedValue_CallsModelSum()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            viewModel.Sum = SumDifferentValue;

            //Assert
            mock.Received().Sum = SumDifferentValue;
        }

        [Fact]
        public void SumSet_SameValue_DoesNtCallModelSum()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;
            viewModel.Sum = SumInitialValue;

            //Act
            viewModel.Sum = SumInitialValue;

            //Assert
            mock.DidNotReceive().Sum = SumInitialValue;
        }

        [Fact]
        public void ParentIdGet_CallsModelParentIdGet()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;

            //Act
            _ = viewModel.ParentId;

            //Assert
            _ = mock.Received().ParentId;
        }

        [Fact]
        public void ParentIdSet_CallsModelParentIdSet()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;

            //Act
            viewModel.ParentId = 69;

            //Assert
            mock.Received().ParentId = 69;
        }

        protected abstract ICategoryViewModel CategoryInitialValue { get; }
        protected abstract ICategoryViewModel CategoryDifferentValue { get; }

        [Fact]
        public void CategoryGet_CallsModelCategoryGet()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;

            //Act
            _ = viewModel.Category;

            //Assert
            _ = mock.Received().CategoryId;
        }

        [Fact]
        public void CategorySet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T viewModel, _) = SubTransIncViewModelFactory;
            viewModel.Category = CategoryInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.Category = CategoryDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.Category), shouldTriggerNotification);
        }

        [Fact]
        public void CategorySet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T viewModel, _) = SubTransIncViewModelFactory;
            viewModel.Category = CategoryInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.Category = CategoryInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.Category), shouldNotTriggerNotification);
        }

        [Fact]
        public void CategorySet_ChangedValue_CallsModelCategory()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;
            viewModel.Category = CategoryInitialValue;

            //Act
            viewModel.Category = CategoryDifferentValue;

            //Assert
            mock.Received().CategoryId = CategoryDifferentValue.Id;
        }

        [Fact]
        public void CategorySet_SameValue_DoesNtCallModelCategory()
        {
            //Arrange
            (T viewModel, ISubTransInc mock) = SubTransIncViewModelFactory;
            viewModel.Category = CategoryInitialValue;

            //Act
            viewModel.Category = CategoryInitialValue;

            //Assert
            mock.DidNotReceive().CategoryId = CategoryInitialValue.Id;
        }
    }
}