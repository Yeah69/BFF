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
    class PagedTitBaseProvider : IPagedSourceProvider<TitLikeViewModel>
    {
        protected readonly IPagedOrm PagedOrm;
        protected readonly IBffOrm Orm;
        protected readonly Account Account;

        public PagedTitBaseProvider(IPagedOrm pagedOrm, Account account, IBffOrm orm)
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

        #region Implementation of IItemSourceProvider<T>

        public PagedSourceItemsPacket<TitLikeViewModel> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            IEnumerable<TitBase> items = PagedOrm.GetPage<TitBase>(pageoffset, count, Account);
            IList<TitLikeViewModel> vmItems = new List<TitLikeViewModel>();
            foreach(TitBase item in items)
            {
                if(item is Transfer)
                    vmItems.Add(new TransferViewModel(item as Transfer, Orm));
                else if (item is ParentTransaction)
                    vmItems.Add(new ParentTransactionViewModel(item as ParentTransaction, Orm));
                else if (item is ParentIncome)
                    vmItems.Add(new ParentIncomeViewModel(item as ParentIncome, Orm));
                else if (item is Transaction)
                    vmItems.Add(new TransactionViewModel(item as Transaction, Orm));
                else if (item is Income)
                    vmItems.Add(new IncomeViewModel(item as Income, Orm));
            }
            return new PagedSourceItemsPacket<TitLikeViewModel> { Items = vmItems , LoadedAt = DateTime.Now };
        }

        public int IndexOf(TitLikeViewModel item)
        {
            return -1; //todo: Find a way
        }

        public int Count => PagedOrm.GetCount<TitBase>(Account);

        #endregion
    }
    class PagedTitBaseProviderAsync : PagedTitBaseProvider, IPagedSourceProviderAsync<TitLikeViewModel>
    {

        public PagedTitBaseProviderAsync(IPagedOrm pagedOrm, Account account, IBffOrm orm) : base(pagedOrm, account, orm)
        {
        }

        #region Implementation of IPagedSourceProviderAsync<TitBase>

        public Task<PagedSourceItemsPacket<TitLikeViewModel>>  GetItemsAtAsync(int pageoffset, int count, bool usePlaceholder)
        {
            return Task.Run(() => GetItemsAt(pageoffset, count, usePlaceholder));
        }

        public TitLikeViewModel GetPlaceHolder(int index, int page, int offset)
        {
            return new TitLikeViewModelPlaceholder(Orm);
        }

        public Task<int> GetCountAsync()
        {
            return Task.Run(() => Count);
        }

        public Task<int> IndexOfAsync(TitLikeViewModel item)
        {
            return Task.Run(() => IndexOf(item));
        }

        #endregion
    }
}
