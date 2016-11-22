using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using Moq;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static Mock<IBffOrm> Mock => CreateMock();

        internal static Mock<IBffOrm> CreateMock(Mock<ICommonPropertyProvider> commonPropertyProviderMock = null, IList<Mock<IAccount>> accountMocks = null,
            IList<Mock<ITransaction>> transactionMocks = null, IList<Mock<IIncome>> incomeMocks = null, IList<Mock<ITransfer>> transferMocks = null,
            IList<Mock<IParentTransaction>> parentTransactionMocks = null, IList<Mock<IParentIncome>> parentIncomeMocks = null,
            IList<Mock<ISubTransaction>> subTransactionMocks = null, IList<Mock<ISubIncome>> subIncomeMocks = null)
        {
            Mock<IBffOrm> mock = new Mock<IBffOrm>();

            /* Verifiable CRUD operations */
            mock.Setup(orm => orm.Insert(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<IAccount>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<IAccount>())).Verifiable();

            mock.Setup(orm => orm.Insert(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<ICategory>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<ICategory>())).Verifiable();

            mock.Setup(orm => orm.Insert(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(orm => orm.Update(It.IsAny<IPayee>())).Verifiable();
            mock.Setup(orm => orm.Delete(It.IsAny<IPayee>())).Verifiable();

            /* Optional CommonPropertyProvider */
            if (commonPropertyProviderMock != null)
                mock.Setup(orm => orm.CommonPropertyProvider).Returns(commonPropertyProviderMock.Object);

            /* Optional GetSubTransInc<>() */
            if (subTransactionMocks != null)
            {
                IEnumerable<long> parentIds = subTransactionMocks.Select(stm => stm.Object.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.Setup(orm => orm.GetSubTransInc<SubTransaction>(parentId)).
                        Returns(() => subTransactionMocks.Where(stm => stm.Object.ParentId == parentId).Select(stm => stm.Object));
                }
            }

            if (subIncomeMocks != null)
            {
                IEnumerable<long> parentIds = subIncomeMocks.Select(stm => stm.Object.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.Setup(orm => orm.GetSubTransInc<SubIncome>(parentId)).
                        Returns(() => subIncomeMocks.Where(stm => stm.Object.ParentId == parentId).Select(stm => stm.Object));
                }
            }

            /* Optional GetAccountBalance() */
            if (accountMocks != null)
            {
                foreach(IAccount account in accountMocks.Select(am => am.Object))
                {
                    long balance = account.StartingBalance;
                    balance += transactionMocks?.Select(tm => tm.Object).Where(t => t.AccountId == account.Id).Sum(t => t.Sum) ?? 0;
                    balance += incomeMocks?.Select(im => im.Object).Where(i => i.AccountId == account.Id).Sum(i => i.Sum) ?? 0;
                    balance += transferMocks?.Select(tm => tm.Object).Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id).
                        Sum(t => t.FromAccountId == account.Id ? -t.Sum : t.Sum) ?? 0;
                    if (parentTransactionMocks != null && subTransactionMocks != null)
                    {
                        IEnumerable<long> parentTransactionIds =
                            parentTransactionMocks.Select(ptm => ptm.Object).Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                        foreach (long parentTransactionId in parentTransactionIds)
                        {
                            balance += subTransactionMocks.Select(stm => stm.Object).Where(st => st.ParentId == parentTransactionId).Sum(st => st.Sum);
                        }
                    }
                    if (parentIncomeMocks != null && subIncomeMocks != null)
                    {
                        IEnumerable<long> parentIncomeIds =
                            parentIncomeMocks.Select(ptm => ptm.Object).Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                        foreach (long parentIncomeId in parentIncomeIds)
                        {
                            balance += subIncomeMocks.Select(stm => stm.Object).Where(st => st.ParentId == parentIncomeId).Sum(st => st.Sum);
                        }
                    }

                    mock.Setup(orm => orm.GetAccountBalance(account)).Returns(balance);
                }
            }

            return mock;
        }
    }
}