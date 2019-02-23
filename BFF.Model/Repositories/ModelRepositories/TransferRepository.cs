using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransferRepository
    {
    }

    internal sealed class TransferRepository : RepositoryBase<ITransfer, ITransSql>, ITransferRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly IFlagRepositoryInternal _flagRepository;

        public TransferRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            IAccountRepositoryInternal accountRepository,
            IFlagRepositoryInternal flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<ITransfer> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            return 
                new Transfer<ITransSql>(
                    persistenceModel,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Date,
                    persistenceModel.Id > 0,
                    persistenceModel.FlagId is null
                        ? null
                        : await _flagRepository.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                    persistenceModel.CheckNumber,
                    persistenceModel.PayeeId is null
                        ? null
                        : await _accountRepository.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                    persistenceModel.CategoryId is null
                        ? null
                        : await _accountRepository.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum,
                    persistenceModel.Cleared == 1L);
        }
    }
}