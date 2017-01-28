using System;
using BFF.MVVM.Models.Native.Structure;
using BFF.Tests.Helper;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native.Structure
{
    public abstract class TitLikeTests<T> : DataModelBaseTests<T> where T : TitLike
    {
        protected abstract T TitLikeFactory { get; }

        protected abstract string MemoInitialValue { get; }
        protected abstract string MemoDifferentValue { get; }

        [Fact]
        public void Memo_ChangeValue_TriggersNotification()
        {
            //Arrange
            T titLike = TitLikeFactory;
            titLike.Memo = MemoInitialValue;

            //Act
            Action shouldTriggerNotification = () => titLike.Memo = MemoDifferentValue;

            //Assert
            Assert.PropertyChanged(titLike, nameof(titLike.Memo), shouldTriggerNotification);
        }

        [Fact]
        public void Memo_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titLike = TitLikeFactory;
            titLike.Memo = MemoInitialValue;

            //Act
            Action shouldTriggerNotification = () => titLike.Memo = MemoInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(titLike, nameof(titLike.Memo), shouldTriggerNotification);
        }
    }
}
