using System.Threading.Tasks;
using BFF.Core;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IPayee : ICommonProperty
    {
        Task MergeToAsync(IPayee payee);
    }
    
    public class Payee : CommonProperty<IPayee>, IPayee
    {
        private readonly IPayeeRepository _repository;

        public Payee(
            IPayeeRepository repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            long id = -1L, 
            string name = "") : base(repository, rxSchedulerProvider, name: name)
        {
            _repository = repository;
            if (id > 0L) Id = id;
        }

        public Task MergeToAsync(IPayee payee)
        {
            return _repository.MergeAsync(from: this, to: payee);
        }
    }
}
