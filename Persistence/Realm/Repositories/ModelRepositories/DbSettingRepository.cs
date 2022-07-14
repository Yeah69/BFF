using BFF.Core.IoC;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmDbSettingRepository : RealmRepositoryBase<IDbSetting, Models.Persistence.DbSetting>, IDbSettingRepository, IScopeInstance
    {
        private readonly ICrudOrm<Models.Persistence.DbSetting> _crudOrm;
        private readonly IRealmOperations _realmOperations;

        public RealmDbSettingRepository(
            ICrudOrm<Models.Persistence.DbSetting> crudOrm,
            IRealmOperations realmOperations) : base(crudOrm)
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
        }

        protected override Task<IDbSetting> ConvertToDomainAsync(Models.Persistence.DbSetting persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            IDbSetting InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.DbSetting(
                    persistenceModel,
                    _crudOrm)
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