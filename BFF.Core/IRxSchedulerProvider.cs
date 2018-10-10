using System.Reactive.Concurrency;

namespace BFF.Core
{
    public interface IRxSchedulerProvider
    {
        IScheduler TimeBasedOperations { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
        // ReSharper disable once InconsistentNaming
        IScheduler UI { get; }
    }
}
