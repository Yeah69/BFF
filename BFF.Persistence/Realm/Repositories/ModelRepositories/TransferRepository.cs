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
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmTransferRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransRealm> crudOrm,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransfer> ConvertToDomainAsync(ITransRealm persistenceModel)
        {
            return 
                new Models.Domain.Transfer(
                    _crudOrm,
                    _rxSchedulerProvider,
                    persistenceModel,
                    true,
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