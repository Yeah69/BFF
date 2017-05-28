﻿using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class TitBaseTests<T> : TitLikeTests<T> where T : TitBase
    {

        protected abstract T TitBaseFactory { get; }

        protected abstract DateTime DateInitialValue { get; }
        protected abstract DateTime DateDifferentValue { get; }

        [Fact]
        public void Date_ChangeValue_TriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            titBase.Date = DateInitialValue;

            //Act
            Action shouldTriggerNotification = () => titBase.Date = DateDifferentValue;

            //Assert
            Assert.PropertyChanged(titBase, nameof(titBase.Date), shouldTriggerNotification);
        }

        [Fact]
        public void Date_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            titBase.Date = DateInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => titBase.Date = DateInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titBase, nameof(titBase.Date), shouldNotTriggerNotification);
        }

        protected abstract bool ClearedInitialValue { get; }
        protected abstract bool ClearedDifferentValue { get; }

        [Fact]
        public void Cleared_ChangeValue_TriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            titBase.Cleared = ClearedInitialValue;

            //Act
            Action shouldTriggerNotification = () => titBase.Cleared = ClearedDifferentValue;

            //Assert
            Assert.PropertyChanged(titBase, nameof(titBase.Cleared), shouldTriggerNotification);
        }

        [Fact]
        public void Cleared_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            titBase.Cleared = ClearedInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => titBase.Cleared = ClearedInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titBase, nameof(titBase.Cleared), shouldNotTriggerNotification);
        }
    }
}