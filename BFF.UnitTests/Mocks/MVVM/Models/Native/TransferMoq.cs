using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class TransferMoq
    {
        public static ITransfer Naked => Substitute.For<ITransfer>();
    }
}