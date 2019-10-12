using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
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

    internal interface IRealmFlagRepositoryInternal : IFlagRepository, IRealmObservableRepositoryBaseInternal<IFlag, Models.Persistence.Flag>
    {
    }

    internal sealed class RealmFlagRepository : RealmObservableRepositoryBase<IFlag, Models.Persistence.Flag>, IRealmFlagRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<Models.Persistence.Flag> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public RealmFlagRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.Flag> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, realmOperations, new FlagComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IFlag> ConvertToDomainAsync(Models.Persistence.Flag persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IFlag InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Flag(
                    _crudOrm,
                    _mergeOrm.Value,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel,
                    Color.FromArgb(
                        (byte)(persistenceModel.Color >> 24 & 0xff),
                        (byte)(persistenceModel.Color >> 16 & 0xff),
                        (byte)(persistenceModel.Color >> 8 & 0xff),
                        (byte)(persistenceModel.Color & 0xff)),
                    persistenceModel.Name);
            }
        }
    }
}
