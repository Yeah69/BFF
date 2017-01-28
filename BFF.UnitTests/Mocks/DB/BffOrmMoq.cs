using System.Collections.Generic;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.DB
{
    public static class BffOrmMoq
    {
        public static IBffOrm Mock => CreateMock();

        public static IBffOrm NakedFake => Substitute.For<IBffOrm>();

        internal static IBffOrm CreateMock(ICommonPropertyProvider commonPropertyProviderMock = null, IList<IAccount> accountMocks = null,
            IList<ITransaction> transactionMocks = null, IList<IIncome> incomeMocks = null, IList<ITransfer> transferMocks = null,
            IList<IParentTransaction> parentTransactionMocks = null, IList<IParentIncome> parentIncomeMocks = null,
            IList<ISubTransaction> subTransactionMocks = null, IList<ISubIncome> subIncomeMocks = null)
        {
            IBffOrm mock = Substitute.For<IBffOrm>();

            if(accountMocks == null) accountMocks = AccountMoq.Mocks;
            if(transactionMocks == null) transactionMocks = TransactionMoq.Mocks;
            if(incomeMocks == null) incomeMocks = IncomeMoq.Mocks;
            if(transferMocks == null) transferMocks = TransferMoq.Mocks;
            if(parentTransactionMocks == null) parentTransactionMocks = ParentTransactionMoq.Mocks;
            if(parentIncomeMocks == null) parentIncomeMocks = ParentIncomeMoq.Mocks;
            if(subTransactionMocks == null) subTransactionMocks = SubTransactionMoq.Mocks;
            if(subIncomeMocks == null) subIncomeMocks = SubIncomeMoq.Mocks;
            if(commonPropertyProviderMock == null) commonPropertyProviderMock = CommonPropertyProviderMoq.CreateMock(accountMocks);

            mock.CommonPropertyProvider.Returns(commonPropertyProviderMock);
            
            IEnumerable<long> parentTransactionIds = subTransactionMocks.Select(stm => stm.ParentId).Distinct();
            foreach (long parentId in parentTransactionIds)
            {
                mock.GetSubTransInc<SubTransaction>(parentId).
                    Returns(subTransactionMocks.Where(stm => stm.ParentId == parentId));
            }
            
            IEnumerable<long> parentIncomeIds = subIncomeMocks.Select(stm => stm.ParentId).Distinct();
            foreach (long parentId in parentIncomeIds)
            {
                mock.GetSubTransInc<SubIncome>(parentId).
                    Returns(subIncomeMocks.Where(stm => stm.ParentId == parentId));
            }
            
            foreach(IAccount account in accountMocks)
            {
                long balance = account.StartingBalance;
                balance += transactionMocks?.Where(t => t.AccountId == account.Id).Sum(t => t.Sum) ?? 0;
                balance += incomeMocks?.Where(i => i.AccountId == account.Id).Sum(i => i.Sum) ?? 0;
                balance += transferMocks?.Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id).
                    Sum(t => t.FromAccountId == account.Id ? -t.Sum : t.Sum) ?? 0;
                if (parentTransactionMocks != null && subTransactionMocks != null)
                {
                    parentTransactionIds = parentTransactionMocks.Select(ptm => ptm).Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                    foreach (long parentTransactionId in parentTransactionIds)
                    {
                        balance += subTransactionMocks.Where(st => st.ParentId == parentTransactionId).Sum(st => st.Sum);
                    }
                }
                if (parentIncomeMocks != null && subIncomeMocks != null)
                {
                    parentIncomeIds = parentIncomeMocks.Where(pt => pt.AccountId == account.Id).Select(pt => pt.Id);
                    foreach (long parentIncomeId in parentIncomeIds)
                    {
                        balance += subIncomeMocks.Where(st => st.ParentId == parentIncomeId).Sum(st => st.Sum);
                    }
                }

                mock.GetAccountBalance(account).Returns(balance);
            }

            return mock;
        }
    }
}