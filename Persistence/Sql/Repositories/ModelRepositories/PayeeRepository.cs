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
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }

    internal interface ISqlitePayeeRepositoryInternal : IPayeeRepository, ISqliteObservableRepositoryBaseInternal<IPayee>
    {
    }

    internal sealed class SqlitePayeeRepository : SqliteObservableRepositoryBase<IPayee, IPayeeSql>, ISqlitePayeeRepositoryInternal
    {
        private readonly ICrudOrm<IPayeeSql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public SqlitePayeeRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IPayeeSql> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeSql persistenceModel)
        {
            return Task.FromResult<IPayee>(new Models.Domain.Payee(
                _crudOrm,
                _mergeOrm.Value,
                this,
                persistenceModel.Id,
                persistenceModel.Name));
        }
    }
}