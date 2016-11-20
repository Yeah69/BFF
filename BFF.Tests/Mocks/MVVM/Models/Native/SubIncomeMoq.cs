using System;
using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.MVVM.Models.Native
{
    public static class SubIncomeMoq
    {
        public static IList<Mock<ISubIncome>> Mocks => new List<Mock<ISubIncome>>
        {
            CreateMock(1, 1, 11, "Salary for July", -110000),
            CreateMock(2, 1, 12, "Bonus July", -20000),
            CreateMock(3, 2, 11, "Salary for August", -100000),
            CreateMock(4, 2, 12, "Bonus August", -13000)
        };

        private static Mock<ISubIncome> CreateMock(long id, long parentId, long categoryId, string memo, long sum)
        {
            Mock<ISubIncome> mock = new Mock<ISubIncome>();

            mock.SetupGet(p => p.Id).Returns(id);
            mock.SetupGet(p => p.ParentId).Returns(parentId);
            mock.SetupGet(p => p.CategoryId).Returns(categoryId);
            mock.SetupGet(p => p.Memo).Returns(memo);
            mock.SetupGet(p => p.Sum).Returns(sum);

            return mock;
        }
    }
}