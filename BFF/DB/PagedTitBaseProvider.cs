using System;
using System.Threading.Tasks;
using AlphaChiTech.Virtualization;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    class PagedTitBaseProvider : IPagedSourceProvider<TitBase>
    {
        protected readonly IPagedOrm Orm;
        protected readonly Account Account;

        public PagedTitBaseProvider(IPagedOrm orm, Account account)
        {
            Orm = orm;
            Account = account;
        }

        #region Implementation of IBaseSourceProvider

        public void OnReset(int count)
        {
        }

        #endregion

        #region Implementation of IItemSourceProvider<T>

        public PagedSourceItemsPacket<TitBase> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            return new PagedSourceItemsPacket<TitBase> { Items = Orm.GetPage<TitBase>(pageoffset, count, Account), LoadedAt = DateTime.Now };
        }

        public int IndexOf(TitBase item)
        {
            return -1; //todo: Find a way
        }

        public int Count => Orm.GetCount<TitBase>(Account);

        #endregion
    }
    class PagedTitBaseProviderAsync : PagedTitBaseProvider, IPagedSourceProviderAsync<TitBase>
    {

        public PagedTitBaseProviderAsync(IPagedOrm orm, Account account) : base(orm, account)
        {
        }

        #region Implementation of IPagedSourceProviderAsync<TitBase>

        public Task<PagedSourceItemsPacket<TitBase>>  GetItemsAtAsync(int pageoffset, int count, bool usePlaceholder)
        {
            return Task.Run(() => GetItemsAt(pageoffset, count, usePlaceholder));
        }

        public TitBase GetPlaceHolder(int index, int page, int offset)
        {
            return new TitBasePlaceholder(DateTime.Now);
        }

        public Task<int> GetCountAsync()
        {
            return Task.Run(() => Count);
        }

        public Task<int> IndexOfAsync(TitBase item)
        {
            return Task.Run(() => IndexOf(item));
        }

        #endregion
    }
}
