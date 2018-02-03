using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Windows.Media;
using BFF.DB.PersistenceModels;
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

    public interface IFlagRepository : IObservableRepositoryBase<Domain.IFlag>
    {
    }

    public sealed class FlagRepository : ObservableRepositoryBase<Domain.IFlag, Flag>, IFlagRepository
    {
        public FlagRepository(IProvideConnection provideConnection, ICrudOrm crudOrm) : base(provideConnection, crudOrm, new FlagComparer()) { }

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

        protected override Converter<(Flag, DbConnection), Domain.IFlag> ConvertToDomain => tuple =>
        {
            (Flag persistenceModel, _) = tuple;
            return new Domain.Flag(
                this,
                persistenceModel.Id, 
                persistenceModel.Name,
                Color.FromArgb(
                    (byte)(persistenceModel.Color >> 24 & 0xff),
                    (byte)(persistenceModel.Color >> 16 & 0xff),
                    (byte)(persistenceModel.Color >> 8 & 0xff),
                    (byte)(persistenceModel.Color & 0xff)));
        };
    }
}
