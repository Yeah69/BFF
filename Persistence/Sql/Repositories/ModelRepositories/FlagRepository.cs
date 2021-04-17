using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public class FlagComparer : Comparer<IFlag>
    {
        public override int Compare(IFlag x, IFlag y)
        {
            return Comparer<string>.Default.Compare(x.Name, y.Name);
        }
    }

    internal interface ISqliteFlagRepositoryInternal : IFlagRepository, ISqliteObservableRepositoryBaseInternal<IFlag>
    {
    }

    internal sealed class SqliteFlagRepository : SqliteObservableRepositoryBase<IFlag, IFlagSql>, ISqliteFlagRepositoryInternal
    {
        private readonly ICrudOrm<IFlagSql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public SqliteFlagRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IFlagSql> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IFlag> ConvertToDomainAsync(IFlagSql persistenceModel)
        {
            return Task.FromResult<IFlag>(
                new Models.Domain.Flag(
                    _crudOrm,
                    _mergeOrm.Value,
                    this,
                    persistenceModel.Id,
                    persistenceModel.Color.ToColor(),
                    persistenceModel.Name));
        }
    }
}
