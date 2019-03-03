using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public class PayeeComparer : Comparer<IPayee>
    {
        public override int Compare(IPayee x, IPayee y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IPayeeRepositoryInternal : IPayeeRepository, IObservableRepositoryBaseInternal<IPayee>
    {
    }

    internal sealed class PayeeRepository : ObservableRepositoryBase<IPayee, IPayeeSql>, IPayeeRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IPayeeSql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public PayeeRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IPayeeSql> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeSql persistenceModel)
        {
            return Task.FromResult<IPayee>(new Models.Domain.Payee(
                _crudOrm,
                _mergeOrm.Value,
                this,
                _rxSchedulerProvider,
                persistenceModel.Id,
                persistenceModel.Name));
        }
    }
}