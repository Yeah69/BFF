using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
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

    public interface IFlagRepository : IObservableRepositoryBase<IFlag>, IMergingRepository<IFlag>
    {
    }

    internal interface IFlagRepositoryInternal : IFlagRepository, IReadOnlyRepository<IFlag>
    {
    }

    internal sealed class FlagRepository : ObservableRepositoryBase<IFlag, IFlagSql>, IFlagRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly Func<IFlagSql> _flagDtoFactory;

        public FlagRepository(
            IProvideSqliteConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm,
            Func<IFlagSql> flagDtoFactory) : base(provideConnection, rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _flagDtoFactory = flagDtoFactory;
        }

        protected override Converter<IFlag, IFlagSql> ConvertToPersistence => domainModel =>
        {
            long color = domainModel.Color.A;
            color = color << 8;
            color = color + domainModel.Color.R;
            color = color << 8;
            color = color + domainModel.Color.G;
            color = color << 8;
            color = color + domainModel.Color.B;

            var flagDto = _flagDtoFactory();

            flagDto.Id = domainModel.Id;
            flagDto.Name = domainModel.Name;
            flagDto.Color = color;

            return flagDto;
        };

        protected override Task<IFlag> ConvertToDomainAsync(IFlagSql persistenceModel)
        {
            return Task.FromResult<IFlag>(
                new Flag(
                    this, 
                    _rxSchedulerProvider,
                    Color.FromArgb(
                        (byte) (persistenceModel.Color >> 24 & 0xff),
                        (byte) (persistenceModel.Color >> 16 & 0xff),
                        (byte) (persistenceModel.Color >> 8 & 0xff),
                        (byte) (persistenceModel.Color & 0xff)),
                    persistenceModel.Id,
                    persistenceModel.Name));
        }

        public async Task MergeAsync(IFlag from, IFlag to)
        {
            await _mergeOrm.MergeFlagAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
