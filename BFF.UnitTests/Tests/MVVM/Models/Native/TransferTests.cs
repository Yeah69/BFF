using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class TransferTests : TitBaseTests<Transfer>
    {
        protected override Transfer DataModelBaseFactory => new Transfer(IdInitialValue, 69, 23, new DateTime(1996, 6, 9), "Yeah, Party!", 69, true);

        protected override long IdInitialValue => 69;

        protected override long IdDifferentValue => 23;

        protected override Transfer TitLikeFactory => new Transfer(1, 69, 23, new DateTime(1996, 6, 9), MemoInitialValue, 69, true);

        protected override string MemoInitialValue => "Yeah, Party!";

        protected override string MemoDifferentValue => "Party, Yeah!";

        protected override Transfer TitBaseFactory => new Transfer(1, 69, 23, DateInitialValue, "Yeah, Party!", 69, ClearedInitialValue);

        protected override DateTime DateInitialValue => new DateTime(1996, 6, 9);

        protected override DateTime DateDifferentValue => new DateTime(1996, 9, 6);

        protected override bool ClearedInitialValue => false;

        protected override bool ClearedDifferentValue => true;


        Transfer TransferFactory => new Transfer(1, FromAccountIdInitialValue, ToAccountIdInitialValue, new DateTime(1996, 6, 9), "Yeah, Party!", SumInitialValue, true);

        long FromAccountIdInitialValue => 69;
        long FromAccountIdDifferentValue => 23;

        long ToAccountIdInitialValue => 3;
        long ToAccountIdDifferentValue => 6;

        long SumInitialValue => 9;
        long SumDifferentValue => 18;

        [Fact]
        public void FromAccountId_ChangeValue_TriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.FromAccountId = FromAccountIdInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.FromAccountId)) notified = true;
            };

            //Act
            transfer.FromAccountId = FromAccountIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void FromAccountId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.FromAccountId = FromAccountIdInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.FromAccountId)) notified = true;
            };

            //Act
            transfer.FromAccountId = FromAccountIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        [Fact]
        public void ToAccountId_ChangeValue_TriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.ToAccountId = ToAccountIdInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.ToAccountId)) notified = true;
            };

            //Act
            transfer.ToAccountId = ToAccountIdDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void ToAccountId_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.ToAccountId = ToAccountIdInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.ToAccountId)) notified = true;
            };

            //Act
            transfer.ToAccountId = ToAccountIdInitialValue;

            //Assert
            Assert.False(notified);
        }

        [Fact]
        public void Sum_ChangeValue_TriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.Sum = SumInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.Sum)) notified = true;
            };

            //Act
            transfer.Sum = SumDifferentValue;

            //Assert
            Assert.True(notified);
        }

        [Fact]
        public void Sum_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            Transfer transfer = TransferFactory;
            bool notified = false;
            transfer.Sum = SumInitialValue;
            transfer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ITransfer.Sum)) notified = true;
            };

            //Act
            transfer.Sum = SumInitialValue;

            //Assert
            Assert.False(notified);
        }
    }
}