using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportAssignments
    {
        public IDictionary<AccountDto, IList<IHaveAccountDto>> AccountToTransactionBase;
        public IDictionary<AccountDto, IList<TransDto>> FromAccountToTransfer;
        public IDictionary<AccountDto, IList<TransDto>> ToAccountToTransfer;

        public IDictionary<PayeeDto, IList<IHavePayeeDto>> PayeeToTransactionBase;

        public IDictionary<TransDto, IList<SubTransactionDto>> ParentTransactionToSubTransaction;

        public IDictionary<FlagDto, IList<IHaveFlagDto>> FlagToTransBase;
    }
}