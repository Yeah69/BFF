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
        Task RunActionAsync(Func<Realms.Realm, Task> action);
        Task<T> RunFuncAsync<T>(Func<Realms.Realm, Task<T>> func);
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
                    var exceptionless = true;
                    try
                    {
                        var realm = _provideConnection.Connection;
                        action(realm);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                        exceptionless = false;
                    }
                    if (exceptionless)
                        tcs.SetResult(Unit.Default);
                });
            return tcs.Task;
        }

        public Task RunActionAsync(Func<Realms.Realm, Task> action)
        {
            var tcs = new TaskCompletionSource<Unit>();
            _eventLoopScheduler.ScheduleAsync(
                async (_, __) =>
                {
                    var exceptionless = true;
                    try
                    {
                        var realm = _provideConnection.Connection;
                        await action(realm).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                        exceptionless = false;
                    }
                    if (exceptionless)
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
                    try
                    {
                        var realm = _provideConnection.Connection;
                        tcs.SetResult(func(realm));
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
            return tcs.Task;
        }

        public Task<T> RunFuncAsync<T>(Func<Realms.Realm, Task<T>> func)
        {
            var tcs = new TaskCompletionSource<T>();
            _eventLoopScheduler.ScheduleAsync(
                async (_, __) =>
                {
                    try
                    {
                        var realm = _provideConnection.Connection;
                        tcs.SetResult(await func(realm).ConfigureAwait(false));
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
            return tcs.Task;
        }

        public void Dispose()
        {
            _eventLoopScheduler.Dispose();
        }
    }
}
