using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
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

    public interface IPayeeRepository : IObservableRepositoryBase<IPayee>
    {
    }

    internal interface IPayeeRepositoryInternal : IPayeeRepository, IMergingRepository<IPayee>, IReadOnlyRepository<IPayee>
    {
    }

    internal sealed class PayeeRepository : ObservableRepositoryBase<IPayee, IPayeeSql>, IPayeeRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;

        public PayeeRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IPayeeSql> crudOrm,
            IMergeOrm mergeOrm) : base(rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeSql persistenceModel)
        {
            return Task.FromResult<IPayee>(new Payee<IPayeeSql>(
                persistenceModel, 
                this,
                this,
                _rxSchedulerProvider,
                persistenceModel.Id > 0, 
                persistenceModel.Name));
        }

        public async Task MergeAsync(IPayee from, IPayee to)
        {
            var fromPersistenceModel = (@from as IDataModelInternal<IPayeeSql>)?.BackingPersistenceModel;
            var toPersistenceModel = (to as IDataModelInternal<IPayeeSql>)?.BackingPersistenceModel;
            if (fromPersistenceModel is null || toPersistenceModel is null) return;
            await _mergeOrm.MergePayeeAsync(fromPersistenceModel, toPersistenceModel).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}