using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.Core.IoC;
using BFF.Model.Models;

namespace BFF.Model
{
    public interface IUpdateBudgetCategory
    {
        void UpdateCategory(ICategory category);
    }
    
    public interface IObserveUpdateBudgetCategory
    {
        IObservable<ICategory> Observe { get; }
    }
    
    internal class UpdateBudgetCategory : IUpdateBudgetCategory, IObserveUpdateBudgetCategory, IDisposable, IOncePerBackend
    {
        private readonly Subject<ICategory> _subject = new Subject<ICategory>(); 

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void UpdateCategory(ICategory category)
        {
            _subject.OnNext(category);
        }

        public IObservable<ICategory> Observe => _subject.AsObservable();
    }
}