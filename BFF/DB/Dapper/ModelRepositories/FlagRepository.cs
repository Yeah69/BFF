using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
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

    public sealed class FlagRepository : ObservableRepositoryBase<IFlag, FlagDto>, IFlagRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;

        public FlagRepository(
            IProvideConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IMergeOrm mergeOrm) : base(provideConnection, rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
        }

        protected override Converter<IFlag, FlagDto> ConvertToPersistence => domainModel =>
        {
            long color = domainModel.Color.A;
            color = color << 8;
            color = color + domainModel.Color.R;
            color = color << 8;
            color = color + domainModel.Color.G;
            color = color << 8;
            color = color + domainModel.Color.B;
            return new FlagDto
            {
                Id = domainModel.Id,
                Name = domainModel.Name,
                Color = color
            };
        };

        protected override Task<IFlag> ConvertToDomainAsync(FlagDto persistenceModel)
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
