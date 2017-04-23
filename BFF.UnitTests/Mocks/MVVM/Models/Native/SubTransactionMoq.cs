using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubTransactionMoq
    {
        public static ISubTransaction Naked => Substitute.For<ISubTransaction>();
    }
}