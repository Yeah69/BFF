using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using BFF.DB.PersistenceModels;
using BFF.Helper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class FlagComparer : Comparer<Domain.IFlag>
    {
        public override int Compare(Domain.IFlag x, Domain.IFlag y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    public interface IFlagRepository : IObservableRepositoryBase<Domain.IFlag>, IMergingRepository<Domain.IFlag>
    {
    }

    public sealed class FlagRepository : ObservableRepositoryBase<Domain.IFlag, Flag>, IFlagRepository
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

        protected override Converter<Domain.IFlag, Flag> ConvertToPersistence => domainModel =>
        {
            long color = domainModel.Color.A;
            color = color << 8;
            color = color + domainModel.Color.R;
            color = color << 8;
            color = color + domainModel.Color.G;
            color = color << 8;
            color = color + domainModel.Color.B;
            return new Flag
            {
                Id = domainModel.Id,
                Name = domainModel.Name,
                Color = color
            };
        };

        protected override Task<Domain.IFlag> ConvertToDomainAsync(Flag persistenceModel)
        {
            return Task.FromResult<Domain.IFlag>(
                new Domain.Flag(
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

        public async Task MergeAsync(Domain.IFlag from, Domain.IFlag to)
        {
            await _mergeOrm.MergeFlagAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);
        }
    }
}
