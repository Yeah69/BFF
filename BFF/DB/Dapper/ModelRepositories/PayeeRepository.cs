using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
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

    public sealed class PayeeRepository : ObservableRepositoryBase<IPayee, PayeeDto>, IPayeeRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;

        public PayeeRepository(
            IProvideConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm) : base(provideConnection, rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
        }
        
        protected override Converter<IPayee, PayeeDto> ConvertToPersistence => domainPayee => 
            new PayeeDto
            {
                Id = domainPayee.Id,
                Name = domainPayee.Name
            };

        protected override Task<IPayee> ConvertToDomainAsync(PayeeDto persistenceModel)
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