using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransactionRepository
    {
    }

    internal sealed class TransactionRepository : RepositoryBase<ITransaction, ITransSql>, ITransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly IFlagRepositoryInternal _flagRepository;

        public TransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            IAccountRepositoryInternal accountRepository,
            ICategoryBaseRepositoryInternal categoryBaseRepository,
            IPayeeRepositoryInternal payeeRepository,
            IFlagRepositoryInternal flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            return new Transaction<ITransSql>(
                persistenceModel,
                this,
                _rxSchedulerProvider,
                persistenceModel.Date,
                persistenceModel.Id > 0,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum,
                persistenceModel.Cleared == 1L);
        }
    }
}