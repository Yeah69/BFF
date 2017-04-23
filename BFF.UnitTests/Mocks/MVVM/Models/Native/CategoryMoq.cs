using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class CategoryMoq
    {
        public static ICategory Naked => Substitute.For<ICategory>();
    }
}