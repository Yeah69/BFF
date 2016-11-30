using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubIncomeMoq
    {
        public static IList<ISubIncome> Mocks => new List<ISubIncome>
        {
            CreateMock(1, 1, 11, "Salary for July", -110000),
            CreateMock(2, 1, 12, "Bonus July", -20000),
            CreateMock(3, 2, 11, "Salary for August", -100000),
            CreateMock(4, 2, 12, "Bonus August", -13000)
        };

        private static ISubIncome CreateMock(long id, long parentId, long categoryId, string memo, long sum)
        {
            ISubIncome mock = Substitute.For<ISubIncome>();
            
            mock.Id.Returns(id);
            mock.ParentId.Returns(parentId);
            mock.CategoryId.Returns(categoryId);
            mock.Memo.Returns(memo);
            mock.Sum.Returns(sum);

            return mock;
        }
    }
}