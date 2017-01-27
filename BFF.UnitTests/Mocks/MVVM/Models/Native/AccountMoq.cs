using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class AccountMoq
    {
        public static IList<IAccount> Mocks => new List<IAccount>
        {
            CreateMock(1, "Bank", 32369),
            CreateMock(2, "Wallet", 6969),
            CreateMock(3, "CreditCard", -2369)
        };

        public static IAccount NotInserted => CreateMock(-1, "Not Inserted Account", 34564356);

        public static IList<IAccount> NotValidToInsert
        {
            get
            {
                IAccount nullName = Substitute.For<IAccount>();
                nullName.Id.Returns(-1);
                nullName.Name.Returns(default(string));
                IAccount emptyName = Substitute.For<IAccount>();
                emptyName.Id.Returns(-1);
                emptyName.Name.Returns("");
                IAccount whitespaceName = Substitute.For<IAccount>();
                whitespaceName.Id.Returns(-1);
                whitespaceName.Name.Returns("    ");
                return new List<IAccount>{ nullName, emptyName, whitespaceName };
            }
        }

        private static IAccount CreateMock(long id, string name, long startingBalance)
        {
            IAccount account = Substitute.For<IAccount>();
            account.Id.Returns(id);
            account.Name.Returns(name);
            account.StartingBalance.Returns(startingBalance);
            return account;
        }
    }
}