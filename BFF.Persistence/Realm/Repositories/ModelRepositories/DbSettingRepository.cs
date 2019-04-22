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

        public RealmDbSettingRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IDbSettingRealm> crudOrm) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(IDbSettingRealm persistenceModel)
        {
            return Task.FromResult<IDbSetting>(
                new Models.Domain.DbSetting(
                    _crudOrm,
                    _rxSchedulerProvider,
                    persistenceModel)
                {
                    CurrencyCultureName = persistenceModel.CurrencyCultureName,
                    DateCultureName = persistenceModel.DateCultureName
                });
        }

        public async Task<IDbSetting> GetSetting()
        {
            return (await FindAllAsync().ConfigureAwait(false)).First();
        }
    }
}