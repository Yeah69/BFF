using BFF.DB;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class CommonPropertyProviderMoq
    {
        public static ICommonPropertyProvider NullAccountViewModel
        {
            get
            {
                ICommonPropertyProvider fake = Substitute.For<ICommonPropertyProvider>();

                fake.GetAccountViewModel(Arg.Any<long>()).Returns(ci => null);

                return fake;
            }
        }

        public static ICommonPropertyProvider NullCategoryViewModel
        {
            get
            {
                ICommonPropertyProvider fake = Substitute.For<ICommonPropertyProvider>();

                fake.GetCategoryViewModel(Arg.Any<long>()).Returns(ci => null);

                return fake;
            }
        }

        public static ICommonPropertyProvider NullPayeeViewModel
        {
            get
            {
                ICommonPropertyProvider fake = Substitute.For<ICommonPropertyProvider>();

                fake.GetPayeeViewModel(Arg.Any<long>()).Returns(ci => null);

                return fake;
            }
        }
    }
}
