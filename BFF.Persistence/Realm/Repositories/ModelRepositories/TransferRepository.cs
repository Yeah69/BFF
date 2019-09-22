using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmTransferRepository : RealmRepositoryBase<ITransfer, ITransRealm>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmTransferRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransRealm> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override Task<ITransfer> ConvertToDomainAsync(ITransRealm persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<ITransfer> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Transfer(
                    _crudOrm,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Date,
                    persistenceModel.Flag is null
                        ? null
                        : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                    persistenceModel.CheckNumber,
                    persistenceModel.FromAccount is null
                        ? null
                        : await _accountRepository.Value.FindAsync(persistenceModel.FromAccount).ConfigureAwait(false),
                    persistenceModel.ToAccount is null
                        ? null
                        : await _accountRepository.Value.FindAsync(persistenceModel.ToAccount).ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum,
                    persistenceModel.Cleared);
            }
        }
    }
}