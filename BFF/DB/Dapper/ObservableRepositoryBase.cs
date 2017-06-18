using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public abstract class ObservableRepositoryBase<TDomain, TPersistence> 
        : CachingRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistanceModel
    {
        private readonly Comparer<TDomain> _comparer;
        public readonly ObservableCollection<TDomain> All;
        
        protected ObservableRepositoryBase(IProvideConnection provideConnection, Comparer<TDomain> comparer) : base(provideConnection)
        {
            _comparer = comparer;
            All = new ObservableCollection<TDomain>(FindAll().OrderBy(o => o, _comparer));
        }

        public override void Add(TDomain dataModel, DbConnection connection = null)
        {
            base.Add(dataModel, connection);
            if(!All.Contains(dataModel))
            {
                int i = 0;
                while(_comparer.Compare(dataModel, All[i]) > 0)
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