using BFF.Core.IoC;
using System;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmTransferRepository : RealmRepositoryBase<ITransfer, Trans>, IScopeInstance
    {
        private readonly ICrudOrm<Trans> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmTransferRepository(
            ICrudOrm<Trans> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override Task<ITransfer> ConvertToDomainAsync(Trans persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<ITransfer> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Transfer(
                    persistenceModel,
                    persistenceModel.Date.UtcDateTime,
                    persistenceModel.Flag is null
                        ? null
                        : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                    persistenceModel.CheckNumber ?? String.Empty,
                    persistenceModel.FromAccount is null
                        ? null
                        : await _accountRepository.Value.FindAsync(persistenceModel.FromAccount).ConfigureAwait(false),
                    persistenceModel.ToAccount is null
                        ? null
                        : await _accountRepository.Value.FindAsync(persistenceModel.ToAccount).ConfigureAwait(false),
                    persistenceModel.Memo ?? String.Empty,
                    persistenceModel.Sum,
                    persistenceModel.Cleared,
                    _crudOrm);
            }
        }
    }
}