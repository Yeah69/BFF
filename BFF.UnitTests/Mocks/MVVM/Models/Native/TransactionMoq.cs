using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransactionMoq
    {
        public static ITransaction Naked => Substitute.For<ITransaction>();
    }
}