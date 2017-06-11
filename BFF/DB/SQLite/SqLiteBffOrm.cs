using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private readonly BffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        public BffRepository BffRepository => _bffRepository;

        public long? GetAccountBalance(IAccount account) => 
            _bffRepository.AccountRepository?.GetBalance(account);

        public long? GetSummaryAccountBalance() =>
            _bffRepository.AccountRepository?.GetBalance(
                new SummaryAccount(_bffRepository.AccountRepository));

        public IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc
        {
            if(typeof(T) == typeof(SubIncome))
                return _bffRepository.SubIncomeRepository?.GetChildrenOf(parentId) 
                       ?? Enumerable.Empty<ISubIncome>();
            if(typeof(T) == typeof(SubTransaction))
                return _bffRepository.SubTransactionRepository?.GetChildrenOf(parentId) 
                       ?? Enumerable.Empty<ISubTransaction>();
            return Enumerable.Empty<ISubTransInc>();
        }

        public SqLiteBffOrm(IProvideConnection provideConnection)
        {
            _bffRepository = new DapperBffRepository(provideConnection);
            
            CommonPropertyProvider = new CommonPropertyProvider(this, _bffRepository);
            ((CommonPropertyProvider) CommonPropertyProvider).InitializeCategoryViewModels();
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
            => _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as Account) as IEnumerable<T>;

        public int GetCount<T>(object specifyingObject = null) => 
            _bffRepository.TitRepository.GetCount(specifyingObject as Account);
    }
}
