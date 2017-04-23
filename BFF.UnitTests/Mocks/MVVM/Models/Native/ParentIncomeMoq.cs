using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class ParentIncomeMoq
    {
        public static IParentIncome Naked => Substitute.For<IParentIncome>();
    }
}