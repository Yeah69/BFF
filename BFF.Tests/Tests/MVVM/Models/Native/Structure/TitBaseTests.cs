using System;
using BFF.MVVM.Models.Native.Structure;
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
            bool notified = false;
            titBase.Date = DateInitialValue;
            titBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitBase.Date)) notified = true;
            };

            //Act
            titBase.Date = DateDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Date_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            bool notified = false;
            titBase.Date = DateInitialValue;
            titBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitBase.Date)) notified = true;
            };

            //Act
            titBase.Date = DateInitialValue;

            //Assert
            Assert.False(notified);
        }

        protected abstract bool ClearedInitialValue { get; }
        protected abstract bool ClearedDifferentValue { get; }

        [Fact]
        public void Cleared_ChangeValue_TriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            bool notified = false;
            titBase.Cleared = ClearedInitialValue;
            titBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitBase.Cleared)) notified = true;
            };

            //Act
            titBase.Cleared = ClearedDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Cleared_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titBase = TitBaseFactory;
            bool notified = false;
            titBase.Cleared = ClearedInitialValue;
            titBase.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitBase.Cleared)) notified = true;
            };

            //Act
            titBase.Cleared = ClearedInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}
