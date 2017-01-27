using BFF.MVVM.Models.Native.Structure;
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
            bool notified = false;
            titLike.Memo = MemoInitialValue;
            titLike.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitLike.Memo)) notified = true;
            };

            //Act
            titLike.Memo = MemoDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Memo_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            T titLike = TitLikeFactory;
            bool notified = false;
            titLike.Memo = MemoInitialValue;
            titLike.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITitLike.Memo)) notified = true;
            };

            //Act
            titLike.Memo = MemoInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}
