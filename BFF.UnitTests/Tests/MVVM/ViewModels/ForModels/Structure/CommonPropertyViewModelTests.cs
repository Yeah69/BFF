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
    public abstract class CommonPropertyViewModelTests<T> : DataModelViewModelTests<T>  where T : CommonPropertyViewModel
    {
        protected abstract (T, ICommonProperty) CommonPropertyViewModelFactory { get; }

        protected abstract string NameInitialValue { get; }
        protected abstract string NameDifferentValue { get; }

        [Fact]
        public void NameGet_CallsModelNameGet()
        {
            //Arrange
            (T viewModel, ICommonProperty mock) = CommonPropertyViewModelFactory;

            //Act
            _ = viewModel.Name;

            //Assert
            _ = mock.Received().Name;
        }

        [Fact]
        public void NameSet_ChangedValue_TriggersNotification()
        {
            //Arrange
            (T viewModel, _) = CommonPropertyViewModelFactory;
            viewModel.Name = NameInitialValue;

            //Act
            Action shouldTriggerNotification = () => viewModel.Name = NameDifferentValue;

            //Assert
            Assert.PropertyChanged(viewModel, nameof(viewModel.Name), shouldTriggerNotification);
        }

        [Fact]
        public void NameSet_SameValue_DoesNtTriggersNotification()
        {
            //Arrange
            (T viewModel, _) = CommonPropertyViewModelFactory;
            viewModel.Name = NameInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => viewModel.Name = NameInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(viewModel, nameof(viewModel.Name), shouldNotTriggerNotification);
        }

        [Fact]
        public void NameSet_ChangedValue_CallsModelName()
        {
            //Arrange
            (T viewModel, ICommonProperty mock) = CommonPropertyViewModelFactory;
            viewModel.Name = NameInitialValue;

            //Act
            viewModel.Name = NameDifferentValue;

            //Assert
            mock.Received().Name = NameDifferentValue;
        }

        [Fact]
        public void NameSet_SameValue_DoesNtCallModelName()
        {
            //Arrange
            (T viewModel, ICommonProperty mock) = CommonPropertyViewModelFactory;
            viewModel.Name = NameInitialValue;

            //Act
            viewModel.Name = NameInitialValue;

            //Assert
            mock.DidNotReceive().Name = NameInitialValue;
        }

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_NotInsertedModel_CallsInsertOnOrm()
        {
            Assert.True(false);
        }

        [Fact]
        public void Insert_AllNonNullCommonPropertyProvider_InsertedModel_DoesNtCallInsertOnOrm()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_InsertedModel_CallsDeleteOnOrm()
        {
            Assert.True(false);
        }

        [Fact]
        public void Delete_NotInsertedModel_DoesntCallDeleteOnOrm()
        {
            Assert.True(false);
        }

        [Fact]
        public void ValidToInsert_AllNonNullCommonPropertyProvider_NotInsertedModel_True()
        {
            Assert.True(false);
        }
    }
}
