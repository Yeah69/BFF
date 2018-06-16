using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper;
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

    public interface IPayeeRepository : IObservableRepositoryBase<Domain.IPayee>
    {
    }

    public sealed class PayeeRepository : ObservableRepositoryBase<Domain.IPayee, Payee>, IPayeeRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        public PayeeRepository(
            IProvideConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm) : base(provideConnection, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
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
    }
}