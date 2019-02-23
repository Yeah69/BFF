using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public class FlagComparer : Comparer<IFlag>
    {
        public override int Compare(IFlag x, IFlag y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IFlagRepository : IObservableRepositoryBase<IFlag>
    {
    }

    internal interface IFlagRepositoryInternal : IFlagRepository, IMergingRepository<IFlag>, IReadOnlyRepository<IFlag>
    {
    }

    internal sealed class FlagRepository : ObservableRepositoryBase<IFlag, IFlagSql>, IFlagRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;

        public FlagRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IFlagSql> crudOrm,
            IMergeOrm mergeOrm) : base(rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IFlag> ConvertToDomainAsync(IFlagSql persistenceModel)
        {
            return Task.FromResult<IFlag>(
                new Flag<IFlagSql>(
                    persistenceModel,
                    this,
                    this,
                    _rxSchedulerProvider,
                    Color.FromArgb(
                        (byte) (persistenceModel.Color >> 24 & 0xff),
                        (byte) (persistenceModel.Color >> 16 & 0xff),
                        (byte) (persistenceModel.Color >> 8 & 0xff),
                        (byte) (persistenceModel.Color & 0xff)),
                    persistenceModel.Id > 0,
                    persistenceModel.Name));
        }

        public async Task MergeAsync(IFlag from, IFlag to)
        {
            var fromPersistenceModel = (@from as IDataModelInternal<IFlagSql>)?.BackingPersistenceModel;
            var toPersistenceModel = (to as IDataModelInternal<IFlagSql>)?.BackingPersistenceModel;
            if (fromPersistenceModel is null || toPersistenceModel is null) return;
            await _mergeOrm.MergeFlagAsync(
                fromPersistenceModel,
                toPersistenceModel).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
