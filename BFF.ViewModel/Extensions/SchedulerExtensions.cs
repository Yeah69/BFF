using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace BFF.ViewModel.Extensions
{
    public static class SchedulerExtensions
    {
        public static IDisposable MinimalSchedule(this IScheduler @this, Action action) =>
            @this.Schedule(Unit.Default, (_, __) =>
            {
                action();
                return Disposable.Empty;
            });

        public static IDisposable MinimalScheduleAsync(this IScheduler @this, Func<Task> func) =>
            @this.ScheduleAsync(Unit.Default, async (_, __, ___) =>
            {
                await func().ConfigureAwait(false);
                return Disposable.Empty;
            });
    }
}
