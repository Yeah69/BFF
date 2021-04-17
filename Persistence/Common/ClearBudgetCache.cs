using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.Core.IoC;
using BFF.Model;

namespace BFF.Persistence.Common
{
    internal interface IObserveClearBudgetCache
    {
        IObservable<Unit> Observe { get; }
    }
    
    public class ClearBudgetCache : IClearBudgetCache, IObserveClearBudgetCache, IDisposable, IOncePerBackend
    {
        private readonly Subject<Unit> _subject = new();

        public void Dispose() => _subject.Dispose();

        public void Clear() => _subject.OnNext(Unit.Default);

        public IObservable<Unit> Observe => _subject.AsObservable();
    }
}