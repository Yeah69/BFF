using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    public class FlagComparer : Comparer<IFlag>
    {
        public override int Compare(IFlag x, IFlag y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IRealmFlagRepositoryInternal : IFlagRepository, IRealmObservableRepositoryBaseInternal<IFlag, IFlagRealm>
    {
    }

    internal sealed class RealmFlagRepository : RealmObservableRepositoryBase<IFlag, IFlagRealm>, IRealmFlagRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IFlagRealm> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public RealmFlagRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IFlagRealm> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IFlag> ConvertToDomainAsync(IFlagRealm persistenceModel)
        {
            return Task.FromResult<IFlag>(
                new Models.Domain.Flag(
                    _crudOrm,
                    _mergeOrm.Value,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel,
                    true,
                    Color.FromArgb(
                        (byte) (persistenceModel.Color >> 24 & 0xff),
                        (byte) (persistenceModel.Color >> 16 & 0xff),
                        (byte) (persistenceModel.Color >> 8 & 0xff),
                        (byte) (persistenceModel.Color & 0xff)),
                    persistenceModel.Name));
        }
    }
}
