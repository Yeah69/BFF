﻿using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IBffOrm : ICrudOrm, IPagedOrm
    {
        ICommonPropertyProvider CommonPropertyProvider { get; }
        BffRepository BffRepository { get; }

        long? GetAccountBalance(IAccount account);
        long? GetSummaryAccountBalance();
        IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc;
    }

    public interface ICrudOrm
    {
        void Insert<T>(T dataModelBase) where T : class, IDataModel;
        T Get<T>(long id) where T : class, IDataModel;
        void Update<T>(T dataModelBase) where T : class, IDataModel;
        void Delete<T>(T dataModelBase) where T : class, IDataModel;
    }
    
    public interface IPagedOrm
    {
        int GetCount<T>(object specifyingObject = null);
        IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
