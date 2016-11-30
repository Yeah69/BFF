using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static IBffOrm Mock => CreateMock();

        internal static IBffOrm CreateMock(ICommonPropertyProvider commonPropertyProviderMock = null, IList<IAccount> accountMocks = null,
            IList<ITransaction> transactionMocks = null, IList<IIncome> incomeMocks = null, IList<ITransfer> transferMocks = null,
            IList<IParentTransaction> parentTransactionMocks = null, IList<IParentIncome> parentIncomeMocks = null,
            IList<ISubTransaction> subTransactionMocks = null, IList<ISubIncome> subIncomeMocks = null)
        {
            IBffOrm mock = Substitute.For<IBffOrm>();

            /* Optional CommonPropertyProvider */
            if (commonPropertyProviderMock != null)
                mock.CommonPropertyProvider.Returns(commonPropertyProviderMock);

            /* Optional GetSubTransInc<>() */
            if (subTransactionMocks != null)
            {
                IEnumerable<long> parentIds = subTransactionMocks.Select(stm => stm.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.GetSubTransInc<SubTransaction>(parentId).
                        Returns(subTransactionMocks.Where(stm => stm.ParentId == parentId));
                }
            }

            if (subIncomeMocks != null)
            {
                IEnumerable<long> parentIds = subIncomeMocks.Select(stm => stm.ParentId).Distinct();
                foreach (long parentId in parentIds)
                {
                    mock.GetSubTransInc<SubIncome>(parentId).
                        Returns(subIncomeMocks.Where(stm => stm.ParentId == parentId));
                }
            }

            /* Optional GetAccountBalance() */
            if (accountMocks != null)
            {
                foreach(IAccount account in accountMocks)
                {
                    long balance = account.StartingBalance;
                    balance += transactionMocks?.Where(t => t.AccountId == account.Id).Sum(t => t.Sum) ?? 0;
                    balance += incomeMocks?.Where(i => i.AccountId == account.Id).Sum(i => i.Sum) ?? 0;
                    balance += transferMocks?.Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id).
                        Sum(t => t.FromAccountId == account.Id ? -t.Sum : t.Sum) ?? 0;
                    if (parentTransactionMocks != null && subTransactionMocks != null)
                    {
                        IEnumerable<long> parentTransactionIds =
                            parentTransactionMocks.Select(ptm => ptm).Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                        foreach (long parentTransactionId in parentTransactionIds)
                        {
                            balance += subTransactionMocks.Where(st => st.ParentId == parentTransactionId).Sum(st => st.Sum);
                        }
                    }
                    if (parentIncomeMocks != null && subIncomeMocks != null)
                    {
                        IEnumerable<long> parentIncomeIds =
                            parentIncomeMocks.Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                        foreach (long parentIncomeId in parentIncomeIds)
                        {
                            balance += subIncomeMocks.Where(st => st.ParentId == parentIncomeId).Sum(st => st.Sum);
                        }
                    }

                    mock.GetAccountBalance(account).Returns(balance);
                }
            }

            return mock;
        }
    }
}