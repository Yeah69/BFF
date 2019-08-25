using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM;

namespace BFF.Persistence.Realm
{
    internal interface IRealmOperations
    {
        Task RunActionAsync(Action<Realms.Realm> action);
        Task<T> RunFuncAsync<T>(Func<Realms.Realm, T> func);
    }

    internal class RealmOperations : IDisposable, IRealmOperations
    {
        private readonly IProvideRealmConnection _provideConnection;
        private readonly EventLoopScheduler _eventLoopScheduler;

        public RealmOperations(
            IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
            _eventLoopScheduler = new EventLoopScheduler();
        }

        public Task RunActionAsync(Action<Realms.Realm> action)
        {
            var tcs = new TaskCompletionSource<Unit>();
            _eventLoopScheduler.Schedule(
                () =>
                {
                    try
                    {
                        var realm = _provideConnection.Connection;
                        action(realm);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                    tcs.SetResult(Unit.Default);
                });
            return tcs.Task;
        }

        public Task<T> RunFuncAsync<T>(Func<Realms.Realm, T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            _eventLoopScheduler.Schedule(
                () =>
                {
                    T result = default;
                    try
                    {
                        var realm = _provideConnection.Connection;
                        result = func(realm);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                    tcs.SetResult(result);
                });
            return tcs.Task;
        }

        public void Dispose()
        {
            _eventLoopScheduler?.Dispose();
        }
    }
}
