using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmTransactionRepository : RealmRepositoryBase<ITransaction, ITransRealm>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransRealm> crudOrm,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransaction> ConvertToDomainAsync(ITransRealm persistenceModel)
        {
            return new Models.Domain.Transaction(
                _crudOrm,
                _rxSchedulerProvider,
                persistenceModel,
                persistenceModel.Date,
                persistenceModel.Flag is null
                    ? null
                    : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.Value.FindAsync(persistenceModel.Account).ConfigureAwait(false),
                persistenceModel.Payee is null
                    ? null
                    : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                persistenceModel.Category is null
                    ? null
                    : await _categoryBaseRepository.Value.FindAsync(persistenceModel.Category).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum,
                persistenceModel.Cleared);
        }
    }
}