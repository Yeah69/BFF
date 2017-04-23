using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class AccountMoq
    {
        public static IAccount Naked => Substitute.For<IAccount>();
    }
}