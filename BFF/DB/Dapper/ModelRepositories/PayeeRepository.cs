using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class PayeeComparer : Comparer<Domain.IPayee>
    {
        public override int Compare(Domain.IPayee x, Domain.IPayee y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IPayeeRepository : IObservableRepositoryBase<Domain.IPayee>, IMergingRepository<Domain.IPayee>
    {
    }

    public sealed class PayeeRepository : ObservableRepositoryBase<Domain.IPayee, Payee>, IPayeeRepository
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
        
        protected override Converter<Domain.IPayee, Payee> ConvertToPersistence => domainPayee => 
            new Payee
            {
                Id = domainPayee.Id,
                Name = domainPayee.Name
            };

        protected override Task<Domain.IPayee> ConvertToDomainAsync(Payee persistenceModel)
        {
            return Task.FromResult<Domain.IPayee>(new Domain.Payee(this, _rxSchedulerProvider, persistenceModel.Id, persistenceModel.Name));
        }

        public async Task MergeAsync(Domain.IPayee from, Domain.IPayee to)
        {
            await _mergeOrm.MergePayeeAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}