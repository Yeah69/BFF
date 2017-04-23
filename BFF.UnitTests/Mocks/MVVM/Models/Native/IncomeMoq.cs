using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class IncomeMoq
    {
        public static IIncome Naked => Substitute.For<IIncome>();
    }
}