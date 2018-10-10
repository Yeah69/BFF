using System.Reactive.Concurrency;
using System.Windows;
using BFF.Core;

namespace BFF.Helper
{
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
