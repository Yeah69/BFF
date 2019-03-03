using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public class FlagComparer : Comparer<IFlag>
    {
        public override int Compare(IFlag x, IFlag y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IFlagRepositoryInternal : IFlagRepository, IObservableRepositoryBaseInternal<IFlag>
    {
    }

    internal sealed class FlagRepository : ObservableRepositoryBase<IFlag, IFlagSql>, IFlagRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IFlagSql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public FlagRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IFlagSql> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
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
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    Color.FromArgb(
                        (byte) (persistenceModel.Color >> 24 & 0xff),
                        (byte) (persistenceModel.Color >> 16 & 0xff),
                        (byte) (persistenceModel.Color >> 8 & 0xff),
                        (byte) (persistenceModel.Color & 0xff)),
                    persistenceModel.Name));
        }
    }
}
