using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public struct ImportAssignments
    {
        public IDictionary<Account, IList<IHaveAccount>> AccountToTransactionBase;
        public IDictionary<Account, IList<Trans>> FromAccountToTransfer;
        public IDictionary<Account, IList<Trans>> ToAccountToTransfer;

        public IDictionary<Payee, IList<IHavePayee>> PayeeToTransactionBase;

        public IDictionary<Trans, IList<SubTransaction>> ParentTransactionToSubTransaction;

        public IDictionary<Flag, IList<IHaveFlag>> FlagToTransBase;
    }
}