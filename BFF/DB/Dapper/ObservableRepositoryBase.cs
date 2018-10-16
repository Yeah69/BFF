using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using MoreLinq;

namespace BFF.DB.Dapper
{
    public interface IObservableRepositoryBase<TDomain> : ICachingRepositoryBase<TDomain> where TDomain : class, IDataModel
    {
        ObservableCollection<TDomain> All { get; }

        IObservable<IEnumerable<TDomain>> ObserveResetAll { get; }
    }

    public abstract class ObservableRepositoryBase<TDomain, TPersistence> 
        : CachingRepositoryBase<TDomain, TPersistence>, IObservableRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelDto
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Comparer<TDomain> _comparer;
        private readonly Task<ObservableCollection<TDomain>> _fetchAll;
        private readonly ISubject<IEnumerable<TDomain>> _observeResetAll;

        public ObservableCollection<TDomain> All =>
            _fetchAll.Result;

        public IObservable<IEnumerable<TDomain>> ObserveResetAll => _observeResetAll.AsObservable();

        protected ObservableRepositoryBase(
            IProvideConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm, 
            Comparer<TDomain> comparer) : base(provideConnection, crudOrm)
        {
            Disposable.Create(() => All.Clear()).AddTo(CompositeDisposable);
            _observeResetAll = new Subject<IEnumerable<TDomain>>().AddHere(CompositeDisposable);
            _rxSchedulerProvider = rxSchedulerProvider;
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
            RemoveFromObservableCollection(dataModel);
        }

        protected void RemoveFromObservableCollection(TDomain dataModel)
        {
            if (All.Contains(dataModel))
                All.Remove(dataModel);
        }

        protected async Task ResetAll()
        {
            await Observable
                .StartAsync(async () => (await FindAllAsync().ConfigureAwait(false)).OrderBy(o => o, _comparer))
                .ObserveOn(_rxSchedulerProvider.UI)
                .Do(all =>
                {
                    All.Clear();
                    all.ForEach(i => All.Add(i));
                })
                .ToTask();
            _observeResetAll.OnNext(All);
        }
    }
}