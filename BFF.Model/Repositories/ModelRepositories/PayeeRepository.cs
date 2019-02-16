using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

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

    internal interface IPayeeRepositoryInternal : IPayeeRepository, IReadOnlyRepository<IPayee>
    {
    }

    internal sealed class PayeeRepository : ObservableRepositoryBase<IPayee, IPayeeSql>, IPayeeRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly Func<IPayeeSql> _payeeDtoFactory;

        public PayeeRepository(
            IProvideSqliteConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            Func<IPayeeSql> payeeDtoFactory) : base(provideConnection, rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _payeeDtoFactory = payeeDtoFactory;
        }
        
        protected override Converter<IPayee, IPayeeSql> ConvertToPersistence => domainPayee =>
        {
            var payeeDto = _payeeDtoFactory();

            payeeDto.Id = domainPayee.Id;
            payeeDto.Name = domainPayee.Name;

            return payeeDto;
        };

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeSql persistenceModel)
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