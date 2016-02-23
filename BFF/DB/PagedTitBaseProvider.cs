using System;
using AlphaChiTech.Virtualization;
using BFF.Model.Native;
using BFF.Model.Native.Structure;

namespace BFF.DB
{
    class PagedTitBaseProvider : IPagedSourceProvider<TitBase>
    {
        private readonly IPagedOrm _orm;
        private readonly Account _account;

        public PagedTitBaseProvider(IPagedOrm orm, Account account)
        {
            _orm = orm;
            _account = account;
        }

        #region Implementation of IBaseSourceProvider

        public void OnReset(int count)
        {
        }

        #endregion

        #region Implementation of IItemSourceProvider<T>

        public PagedSourceItemsPacket<TitBase> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {
            return new PagedSourceItemsPacket<TitBase> { Items = _orm.GetPage<TitBase>(pageoffset, count, _account), LoadedAt = DateTime.Now };
        }

        public int IndexOf(TitBase item)
        {
            return -1;
        }

        public int Count => _orm.GetCount<TitBase>(_account);

        #endregion
    }
}
