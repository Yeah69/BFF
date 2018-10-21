using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportAssignments
    {
        public IDictionary<IAccountDto, IList<IHaveAccountDto>> AccountToTransactionBase;
        public IDictionary<IAccountDto, IList<ITransDto>> FromAccountToTransfer;
        public IDictionary<IAccountDto, IList<ITransDto>> ToAccountToTransfer;

        public IDictionary<IPayeeDto, IList<IHavePayeeDto>> PayeeToTransactionBase;

        public IDictionary<ITransDto, IList<ISubTransactionDto>> ParentTransactionToSubTransaction;

        public IDictionary<IFlagDto, IList<IHaveFlagDto>> FlagToTransBase;
    }
}