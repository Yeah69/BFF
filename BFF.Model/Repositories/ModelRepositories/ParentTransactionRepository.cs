using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IParentTransactionRepository
    {
    }

    internal sealed class ParentTransactionRepository : RepositoryBase<IParentTransaction, ITransSql>, IParentTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionRepository;
        private readonly IFlagRepositoryInternal _flagRepository;

        public ParentTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            IAccountRepositoryInternal accountRepository,
            IPayeeRepositoryInternal payeeRepository,
            ISubTransactionRepository subTransactionRepository,
            IFlagRepositoryInternal flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<IParentTransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            var parentTransaction = new ParentTransaction<ITransSql>(
                persistenceModel,
                this,
                _rxSchedulerProvider,
                await _subTransactionRepository.GetChildrenOfAsync(persistenceModel.Id).ConfigureAwait(false),
                persistenceModel.Date,
                persistenceModel.Id > 0,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Cleared == 1L);

            foreach (var subTransaction in parentTransaction.SubTransactions)
            {
                subTransaction.Parent = parentTransaction;
            }

            return parentTransaction;
        }
    }
}