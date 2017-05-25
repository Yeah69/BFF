using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Helper;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class AccountTests : CommonPropertyTests<Account>
    {
        protected override Account DataModelBaseFactory => new Account(IdInitialValue, "Bank", 123);
        protected override long IdInitialValue => 69;
        protected override long IdDifferentValue => 23;

        protected override Account CommonPropertyFactory => new Account(1, NameInitialValue, 123);
        protected override string NameInitialValue => "Cash";
        protected override string NameDifferentValue => "Sparkasse";
        protected override string ToStringExpectedValue => "Cash";

        Account AccountFactory => new Account(1, "Bank", StartingBalanceInitialValue);

        long StartingBalanceInitialValue => 690;
        long StartingBalanceDifferentValue => 230;

        [Fact]
        public void StartingBalance_ChangeValue_TriggersNotification()
        {
            //Arrange
            Account account = AccountFactory;
            account.StartingBalance = StartingBalanceInitialValue;

            //Act
            Action shouldTriggerNotification = () => account.StartingBalance = StartingBalanceDifferentValue;

            //Assert
            Assert.PropertyChanged(account, nameof(account.StartingBalance), shouldTriggerNotification);
        }

        [Fact]
        public void StartingBalance_SameValue_DoesntTriggersNotification()
        {
            //Arrange
            Account account = AccountFactory;
            account.StartingBalance = StartingBalanceInitialValue;

            //Act
            Action shouldNotTriggerNotification = () => account.StartingBalance = StartingBalanceInitialValue;

            //Assert
            NativeAssert.DoesNotRaisePropertyChanged(account, nameof(account.StartingBalance), shouldNotTriggerNotification);
        }
    }
}
