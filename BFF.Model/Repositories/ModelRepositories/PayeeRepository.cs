using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public class PayeeComparer : Comparer<IPayee>
    {
        public override int Compare(IPayee x, IPayee y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IPayeeRepository : IObservableRepositoryBase<IPayee>, IMergingRepository<IPayee>
    {
    }

    internal sealed class PayeeRepository : ObservableRepositoryBase<IPayee, IPayeeDto>, IPayeeRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly Func<IPayeeDto> _payeeDtoFactory;

        public PayeeRepository(
            IProvideConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            Func<IPayeeDto> payeeDtoFactory) : base(provideConnection, rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _payeeDtoFactory = payeeDtoFactory;
        }
        
        protected override Converter<IPayee, IPayeeDto> ConvertToPersistence => domainPayee =>
        {
            var payeeDto = _payeeDtoFactory();

            payeeDto.Id = domainPayee.Id;
            payeeDto.Name = domainPayee.Name;

            return payeeDto;
        };

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeDto persistenceModel)
        {
            return Task.FromResult<IPayee>(new Payee(this, _rxSchedulerProvider, persistenceModel.Id, persistenceModel.Name));
        }

        public async Task MergeAsync(IPayee from, IPayee to)
        {
            await _mergeOrm.MergePayeeAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}