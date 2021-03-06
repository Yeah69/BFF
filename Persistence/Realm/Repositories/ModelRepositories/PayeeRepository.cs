using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    public class PayeeComparer : Comparer<IPayee>
    {
        public override int Compare(IPayee? x, IPayee? y)
        {
            return Comparer<string>.Default.Compare(x?.Name, y?.Name);
        }
    }

    internal interface IRealmPayeeRepositoryInternal : IPayeeRepository, IRealmObservableRepositoryBaseInternal<IPayee, Models.Persistence.Payee>
    {
    }

    internal sealed class RealmPayeeRepository : RealmObservableRepositoryBase<IPayee, Models.Persistence.Payee>, IRealmPayeeRepositoryInternal
    {
        private readonly ICrudOrm<Models.Persistence.Payee> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IMergeOrm> _mergeOrm;

        public RealmPayeeRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.Payee> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IMergeOrm> mergeOrm) : base(rxSchedulerProvider, crudOrm, realmOperations, new PayeeComparer())
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _mergeOrm = mergeOrm;
        }

        protected override Task<IPayee> ConvertToDomainAsync(Models.Persistence.Payee persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IPayee InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Payee(
                    persistenceModel,
                    persistenceModel.Name,
                    _crudOrm,
                    _mergeOrm.Value,
                    this);
            }
        }
    }
}