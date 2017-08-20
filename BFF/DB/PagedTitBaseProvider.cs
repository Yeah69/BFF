using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.DB
{
    class PagedTitBaseProvider : IPagedSourceProvider<ITitLikeViewModel>
    {
        protected readonly IPagedOrm PagedOrm;
        protected readonly IBffOrm Orm;
        protected readonly IAccount Account;

        public PagedTitBaseProvider(IPagedOrm pagedOrm, IAccount account, IBffOrm orm)
        {
            PagedOrm = pagedOrm;
            Account = account;
            Orm = orm;
        }

        #region Implementation of IBaseSourceProvider

        public void OnReset(int count)
        {
        }

        #endregion

        protected PagedSourceItemsPacket<ITitLikeViewModel> CreatePacket(IEnumerable<ITitBase> items)
        {
            IList<ITitLikeViewModel> vmItems = new List<ITitLikeViewModel>();
            foreach (ITitBase item in items)
            {
                switch (item)
                {
                    case ITransfer transfer:
                        vmItems.Add(new TransferViewModel(transfer, Orm, Orm.CommonPropertyProvider.AccountViewModelService));
                        break;
                    case IParentTransaction parentTransaction:
                        vmItems.Add(new ParentTransactionViewModel(parentTransaction, Orm, Orm.SubTransactionViewModelService));
                        break;
                    case IParentIncome parentIncome:
                        vmItems.Add(new ParentIncomeViewModel(
                            parentIncome,
                            Orm,
                            Orm.SubIncomeViewModelService));
                        break;
                    case ITransaction transaction:
                        vmItems.Add(new TransactionViewModel(
                            transaction,
                            Orm,
                            Orm.CommonPropertyProvider.AccountViewModelService,
                            Orm.CommonPropertyProvider.PayeeViewModelService,
                            Orm.CommonPropertyProvider.CategoryViewModelService));
                        break;
                    case IIncome income:
                        vmItems.Add(new IncomeViewModel(
                            income,
                            Orm,
                            Orm.CommonPropertyProvider.AccountViewModelService,
                            Orm.CommonPropertyProvider.PayeeViewModelService,
                            Orm.CommonPropertyProvider.CategoryViewModelService));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return new PagedSourceItemsPacket<ITitLikeViewModel> { Items = vmItems, LoadedAt = DateTime.Now };
        }

        #region Implementation of IItemSourceProvider<T>

        public PagedSourceItemsPacket<ITitLikeViewModel> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            IEnumerable<ITitBase> items = PagedOrm.GetPage<ITitBase>(pageoffset, count, Account);
            return CreatePacket(items);
        }

        public int IndexOf(ITitLikeViewModel item)
        {
            return -1; //todo: Find a way
        }

        public int Count => PagedOrm.GetCount<ITitBase>(Account);

        #endregion
    }
    class PagedTitBaseProviderAsync : PagedTitBaseProvider, IPagedSourceProviderAsync<ITitLikeViewModel>
    {
        private readonly IViewRepositoryAsync<ITitBase, IAccount> _repository;

        public PagedTitBaseProviderAsync(IViewRepositoryAsync<ITitBase, IAccount> repository, IPagedOrm pagedOrm,  IAccount account, IBffOrm orm) : base(pagedOrm, account, orm)
        {
            _repository = repository;
        }

        #region Implementation of IPagedSourceProviderAsync<TitBase>

        public async Task<PagedSourceItemsPacket<ITitLikeViewModel>>  GetItemsAtAsync(int pageOffset, int count, bool usePlaceholder)
        {
            IEnumerable<ITitBase> items = await _repository.GetPageAsync(pageOffset, count, Account);
            return CreatePacket(items);
        }

        public ITitLikeViewModel GetPlaceHolder(int index, int page, int offset)
        {
            return new TitLikeViewModelPlaceholder();
        }

        public async Task<int> GetCountAsync()
        {
            return await _repository.GetCountAsync(Account);
        }

        public async Task<int> IndexOfAsync(ITitLikeViewModel item)
        {
            return await Task.Run(() => IndexOf(item)).ConfigureAwait(false);
        }

        #endregion
    }
}
