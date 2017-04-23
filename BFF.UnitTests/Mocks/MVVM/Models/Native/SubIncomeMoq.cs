using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubIncomeMoq
    {
        public static ISubIncome Naked => Substitute.For<ISubIncome>();
    }
}