using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public interface IObservableRepositoryBase<TDomain> : ICachingRepositoryBase<TDomain> where TDomain : class, IDataModel
    {
        ObservableCollection<TDomain> All { get; }
    }

    public abstract class ObservableRepositoryBase<TDomain, TPersistence> 
        : CachingRepositoryBase<TDomain, TPersistence>, IObservableRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
        private readonly Comparer<TDomain> _comparer;
        public ObservableCollection<TDomain> All { get; }

        protected ObservableRepositoryBase(IProvideConnection provideConnection, Comparer<TDomain> comparer) : base(provideConnection)
        {
            _comparer = comparer;
            All = new ObservableCollection<TDomain>(FindAll().OrderBy(o => o, _comparer));
        }

        public sealed override IEnumerable<TDomain> FindAll(DbConnection connection = null)
        {
            return base.FindAll(connection);
        }

        public override void Add(TDomain dataModel, DbConnection connection = null)
        {
            base.Add(dataModel, connection);
            if(!All.Contains(dataModel))
            {
                int i = 0;
                while(i < All.Count && _comparer.Compare(dataModel, All[i]) > 0)
                    i++;
                All.Insert(i, dataModel);
            }
        }

        public override void Delete(TDomain dataModel, DbConnection connection = null)
        {
            base.Delete(dataModel, connection);
            if(All.Contains(dataModel))
            {
                All.Remove(dataModel);
            }
        }
    }
}