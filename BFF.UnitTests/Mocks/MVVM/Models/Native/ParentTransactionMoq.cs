using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class ParentTransactionMoq
    {
        public static IParentTransaction Naked => Substitute.For<IParentTransaction>();
    }
}