using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.Core.IoC;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.ViewModel.Managers
{
    public interface IBudgetRefreshes
    {
        IObservable<Unit> ObserveCompleteRefreshes { get; }
        
        IObservable<Unit> ObserveHeadersRefreshes { get; }

        IObservable<Unit> ObserveCategoryRefreshes(ICategoryViewModel category);
        
        void Refresh(ICategoryViewModel category);
        
        void RefreshCompletely();
    }
    
    internal class BudgetRefreshes : IBudgetRefreshes, IOncePerBackend, IDisposable
    {
        private readonly ConcurrentDictionary<ICategoryViewModel, Subject<Unit>> _categoryEvents = 
            new();
        
        private readonly Subject<Unit> _monthRefreshes;

        private readonly Subject<Unit> _completeRefreshes;
        
        private readonly CompositeDisposable _compositeDisposable = new();

        public BudgetRefreshes()
        {
            _monthRefreshes = new Subject<Unit>().CompositeDisposalWith(_compositeDisposable);
            _completeRefreshes = new Subject<Unit>().CompositeDisposalWith(_compositeDisposable);
        }

        public IObservable<Unit> ObserveCompleteRefreshes => _completeRefreshes.AsObservable();
        public IObservable<Unit> ObserveHeadersRefreshes => _monthRefreshes.AsObservable();

        public IObservable<Unit> ObserveCategoryRefreshes(ICategoryViewModel category) => 
            GetOrAddCategorySubject(category).AsObservable();

        public void Refresh(ICategoryViewModel category)
        {
            GetOrAddCategorySubject(category).OnNextUnit();
            _monthRefreshes.OnNextUnit();
        }

        public void RefreshCompletely()
        {
            _completeRefreshes.OnNextUnit();
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        private Subject<Unit> GetOrAddCategorySubject(ICategoryViewModel category) =>
            _categoryEvents.GetOrAdd(category, c => new Subject<Unit>())
                .CompositeDisposalWith(_compositeDisposable);
    }
}