using BFF.DB;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static IBffOrm Naked => Substitute.For<IBffOrm>();
    }
}