using BFF.MVVM.ViewModels.ForModels;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class CategoryViewModelMoq
    {
        public static ICategoryViewModel Naked => Substitute.For<ICategoryViewModel>();
    }
}