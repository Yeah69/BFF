using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    public class PayeeComparer : Comparer<IPayee>
    {
        public override int Compare(IPayee x, IPayee y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IRealmPayeeRepositoryInternal : IPayeeRepository, IRealmObservableRepositoryBaseInternal<IPayee, IPayeeRealm>
    {
    }

    internal sealed class RealmPayeeRepository : RealmObservableRepositoryBase<IPayee, IPayeeRealm>, IRealmPayeeRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IPayeeRealm> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public RealmPayeeRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IPayeeRealm> crudOrm,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, new PayeeComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IPayee> ConvertToDomainAsync(IPayeeRealm persistenceModel)
        {
            return Task.FromResult<IPayee>(new Models.Domain.Payee(
                _crudOrm,
                _mergeOrm.Value,
                this,
                _rxSchedulerProvider,
                persistenceModel,
                persistenceModel.Name));
        }
    }
}