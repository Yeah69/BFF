using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Models.Persistence;
using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;
using MuVaViMo;

namespace BFF.Persistence.Realm.Repositories
{

    internal interface
        IRealmObservableRepositoryBaseInternal<TDomain, TPersistence> : IObservableRepositoryBase<TDomain>, IRealmCachingRepositoryBase<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        void RemoveFromObservableCollection(TDomain dataModel);
        Task ResetAll();
    }

    internal abstract class RealmObservableRepositoryBase<TDomain, TPersistence>
        : RealmCachingRepositoryBase<TDomain, TPersistence>, IRealmObservableRepositoryBaseInternal<TDomain, TPersistence>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Comparer<TDomain> _comparer;
        private readonly ISubject<IEnumerable<TDomain>> _observeResetAll;

        private ObservableCollection<TDomain> _backingObservableCollection;

        public IObservableReadOnlyList<TDomain> All { get; }
        public Task<IDeferredObservableReadOnlyList<TDomain>> AllAsync { get; }

        public IObservable<IEnumerable<TDomain>> ObserveResetAll => _observeResetAll.AsObservable();

        protected RealmObservableRepositoryBase(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<TPersistence> crudOrm,
            IRealmOperations realmOperations,
            Comparer<TDomain> comparer) : base(crudOrm, realmOperations)
        {
            Disposable.Create(() => _backingObservableCollection.Clear()).AddTo(CompositeDisposable);
            _observeResetAll = new Subject<IEnumerable<TDomain>>().AddForDisposalTo(CompositeDisposable);
            _rxSchedulerProvider = rxSchedulerProvider;
            _comparer = comparer;
            var collection = Task.Run(FetchAll).ToObservableReadOnlyList();
            All = collection;
            AllAsync = collection.InitializedCollectionAsync;
        }

        private async Task<ObservableCollection<TDomain>> FetchAll()
        {
            _backingObservableCollection = new ObservableCollection<TDomain>(
                (await FindAllAsync().ConfigureAwait(false)).OrderBy(Basic.Identity, _comparer));
            return _backingObservableCollection;
        }

        protected sealed override Task<IEnumerable<TDomain>> FindAllAsync()
        {
            return base.FindAllAsync();
        }

        public override async Task<bool> AddAsync(TDomain dataModel)
        {
            if (_backingObservableCollection is null)
                throw new Exception("Collection not initialized yet. Cannot add items.");
            var result = await base.AddAsync(dataModel).ConfigureAwait(false);
            if (!_backingObservableCollection.Contains(dataModel))
            {
                int i = 0;
                while (i < _backingObservableCollection.Count && _comparer.Compare(dataModel, _backingObservableCollection[i]) > 0)
                    i++;
                _backingObservableCollection.Insert(i, dataModel);
            }

            return result;
        }

        public void RemoveFromObservableCollection(TDomain dataModel)
        {
            if (_backingObservableCollection is null)
                throw new Exception("Collection not initialized yet. Cannot remove items.");
            if (_backingObservableCollection.Contains(dataModel))
                _backingObservableCollection.Remove(dataModel);
        }

        public async Task ResetAll()
        {
            await Observable
                .StartAsync(async () => (await FindAllAsync().ConfigureAwait(false)).OrderBy(Basic.Identity, _comparer))
                .ObserveOn(_rxSchedulerProvider.UI)
                .Do(all =>
                {
                    _backingObservableCollection?.Clear();
                    if (_backingObservableCollection is null)
                        throw new Exception("Collection not initialized yet. Cannot add items.");
                    all.ForEach(i => _backingObservableCollection.Add(i));
                })
                .ToTask()
                .ConfigureAwait(false);
            _observeResetAll.OnNext(All);
        }
    }
}