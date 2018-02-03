using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<TDomain> _all;

        public ObservableCollection<TDomain> All =>
            _all ?? (_all = new ObservableCollection<TDomain>(FindAll().OrderBy(o => o, _comparer)));

        protected ObservableRepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm, Comparer<TDomain> comparer) : base(provideConnection, crudOrm)
        {
            _comparer = comparer;
        }

        public sealed override IEnumerable<TDomain> FindAll()
        {
            return base.FindAll();
        }

        public override void Add(TDomain dataModel)
        {
            base.Add(dataModel);
            if(!_all.Contains(dataModel))
            {
                int i = 0;
                while(i < _all.Count && _comparer.Compare(dataModel, _all[i]) > 0)
                    i++;
                _all.Insert(i, dataModel);
            }
        }

        public override void Delete(TDomain dataModel)
        {
            base.Delete(dataModel);
            if(_all.Contains(dataModel))
            {
                _all.Remove(dataModel);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _all.Clear();
            }
        }
    }
}