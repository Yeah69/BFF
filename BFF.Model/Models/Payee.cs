using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface IPayee : ICommonProperty
    {
        Task MergeToAsync(IPayee payee);
    }

    internal class Payee<TPersistence> : CommonProperty<IPayee, TPersistence>, IPayee
        where TPersistence : class, IPersistenceModel
    {
        private readonly IMergingRepository<IPayee> _mergingRepository;

        public Payee(
            TPersistence backingPersistenceModel,
            IMergingRepository<IPayee> mergingRepository,
            IRepository<IPayee, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            bool isInserted, 
            string name = "") : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, name: name)
        {
            _mergingRepository = mergingRepository;
        }

        public Task MergeToAsync(IPayee payee)
        {
            return _mergingRepository.MergeAsync(@from: this, to: payee);
        }
    }
}
