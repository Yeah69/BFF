﻿using System;
using BFF.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class SubTransIncTests<T> : TitLikeTests<T> where T : SubTransInc
    {
        protected abstract T SubTransIncFactory { get; }

        protected abstract long ParentIdInitialValue { get; }
        protected abstract long ParentIdDifferentValue { get; }

        [Fact]
        public void ParentId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.ParentId = ParentIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.ParentId = ParentIdDifferentValue;

            //Assert
            Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.ParentId), shouldTriggerNotification);
        }

        [Fact]
        public void ParentId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.ParentId = ParentIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.ParentId = ParentIdInitialValue;

            //Assert
            Assert.Throws<Xunit.Sdk.PropertyChangedException>(
                () => Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.ParentId), shouldTriggerNotification)
            );
        }

        protected abstract long CategoryIdInitialValue { get; }
        protected abstract long CategoryIdDifferentValue { get; }

        [Fact]
        public void CategoryId_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.CategoryId = CategoryIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.CategoryId = CategoryIdDifferentValue;

            //Assert
            Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.CategoryId), shouldTriggerNotification);
        }

        [Fact]
        public void CategoryId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.CategoryId = CategoryIdInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.CategoryId = CategoryIdInitialValue;

            //Assert
            Assert.Throws<Xunit.Sdk.PropertyChangedException>(
                () => Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.CategoryId), shouldTriggerNotification)
            );
        }

        protected abstract long SumInitialValue { get; }
        protected abstract long SumDifferentValue { get; }

        [Fact]
        public void Sum_ChangeValue_TriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.Sum = SumDifferentValue;

            //Assert
            Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.Sum), shouldTriggerNotification);
        }

        [Fact]
        public void Sum_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T subTransInc = SubTransIncFactory;
            subTransInc.Sum = SumInitialValue;

            //Act
            Action shouldTriggerNotification = () => subTransInc.Sum = SumInitialValue;

            //Assert
            Assert.Throws<Xunit.Sdk.PropertyChangedException>(
                () => Assert.PropertyChanged(subTransInc, nameof(ISubTransInc.Sum), shouldTriggerNotification)
            );
        }
    }
}
