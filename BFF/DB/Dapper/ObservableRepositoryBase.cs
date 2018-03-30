using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly Task<ObservableCollection<TDomain>> _fetchAll;

        public ObservableCollection<TDomain> All =>
            _fetchAll.Result;

        protected ObservableRepositoryBase(IProvideConnection provideConnection, ICrudOrm crudOrm, Comparer<TDomain> comparer) : base(provideConnection, crudOrm)
        {
            _comparer = comparer;
            _fetchAll = Task.Run(FetchAll);
        }

        private async Task<ObservableCollection<TDomain>> FetchAll()
        {
            return new ObservableCollection<TDomain>((await FindAllAsync().ConfigureAwait(false)).OrderBy(o => o, _comparer));
        }

        public sealed override Task<IEnumerable<TDomain>> FindAllAsync()
        {
            return base.FindAllAsync();
        }

        public override async Task AddAsync(TDomain dataModel)
        {
            await base.AddAsync(dataModel).ConfigureAwait(false);
            if(!All.Contains(dataModel))
            {
                int i = 0;
                while(i < All.Count && _comparer.Compare(dataModel, All[i]) > 0)
                    i++;
                All.Insert(i, dataModel);
            }
        }

        public override async Task DeleteAsync(TDomain dataModel)
        {
            await base.DeleteAsync(dataModel).ConfigureAwait(false);
            if(All.Contains(dataModel))
            {
                All.Remove(dataModel);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                All.Clear();
            }
        }
    }
}