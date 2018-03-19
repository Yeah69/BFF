using System.Reactive.Concurrency;
using System.Windows;

namespace BFF.Helper
{
    public interface IRxSchedulerProvider
    {
        IScheduler TimeBasedOperations { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
        IScheduler UI { get; }
    }

    class WpfRxSchedulerProvider : IRxSchedulerProvider
    {
        public WpfRxSchedulerProvider()
        {
            TimeBasedOperations = DefaultScheduler.Instance;
            Task = TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
            UI = new DispatcherScheduler(Application.Current.Dispatcher);
        }

        public IScheduler TimeBasedOperations { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler UI { get; }
    }
}
