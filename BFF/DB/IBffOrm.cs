using System;
using System.Collections.Generic;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IBffOrm : ICrudOrm, IPagedOrm
    {
        ICommonPropertyProvider CommonPropertyProvider { get; }

        string DbPath { get; }
        void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments);
        long? GetAccountBalance(IAccount account);
        long? GetSummaryAccountBalance();
        IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc;
    }

    public interface ICrudOrm
    {
        IEnumerable<T> GetAll<T>() where T : class, IDataModel;
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
