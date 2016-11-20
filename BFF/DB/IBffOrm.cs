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
        IEnumerable<ITitBase> GetAllTits(DateTime startTime, DateTime endTime, IAccount account = null);
        long? GetAccountBalance(IAccount account);
        long? GetSummaryAccountBalance();
        IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc;
    }

    public interface ICrudOrm
    {
        IEnumerable<T> GetAll<T>() where T : class, IDataModelBase;
        long Insert<T>(T dataModelBase) where T : class, IDataModelBase;
        T Get<T>(long id) where T : class, IDataModelBase;
        void Update<T>(T dataModelBase) where T : class, IDataModelBase;
        void Delete<T>(T dataModelBase) where T : class, IDataModelBase;
    }
    
    public interface IPagedOrm
    {
        int GetCount<T>(object specifyingObject = null);
        IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
