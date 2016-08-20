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
        IEnumerable<TitBase> GetAllTits(DateTime startTime, DateTime endTime, Account account = null);
        long? GetAccountBalance(Account account = null);
        IEnumerable<T> GetSubTransInc<T>(long parentId) where T : ISubTransInc;
    }

    public interface ICrudOrm
    {
        IEnumerable<T> GetAll<T>() where T : DataModelBase;
        long Insert<T>(T dataModelBase) where T : DataModelBase;
        T Get<T>(long id) where T : DataModelBase;
        void Update<T>(T dataModelBase) where T : DataModelBase;
        void Delete<T>(T dataModelBase) where T : DataModelBase;
    }
    
    public interface IPagedOrm
    {
        int GetCount<T>(object specifyingObject = null);
        IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null);
    }
}
