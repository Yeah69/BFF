using BFF.Core.IoC;
using BFF.Model.Models;

namespace BFF.Model.Repositories
{
    public interface ICreateNewModels : IOncePerBackend
    {
        ITransaction CreateTransaction();
        ITransfer CreateTransfer();
        IParentTransaction CreateParentTransfer();
        ISubTransaction CreateSubTransaction();
        IAccount CreateAccount();
    }
}
