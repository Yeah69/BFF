using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmDbSettingRepository : RealmRepositoryBase<IDbSetting, IDbSettingRealm>, IDbSettingRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IDbSettingRealm> _crudOrm;
        private readonly IRealmOperations _realmOperations;

        public RealmDbSettingRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IDbSettingRealm> crudOrm,
            IRealmOperations realmOperations) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingRealm persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IDbSetting InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.DbSetting(
                    _crudOrm,
                    _rxSchedulerProvider,
                    persistenceModel)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                };
            }
        }

        public async Task<IDbSetting> GetSetting()
        {
            return (await FindAllAsync().ConfigureAwait(false)).First();
        }
    }
}