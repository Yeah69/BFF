using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class CategoryViewModelMoq
    {
        public static ICategoryViewModel Naked => Substitute.For<ICategoryViewModel>();
    }
}