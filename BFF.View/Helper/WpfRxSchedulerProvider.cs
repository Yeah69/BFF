using System.Reactive.Concurrency;
using System.Windows;
using BFF.Core.Helper;
using System.Windows.Threading;

namespace BFF.View.Helper
{
    class WpfRxSchedulerProvider : IRxSchedulerProvider
    {
        public WpfRxSchedulerProvider()
        {
            TimeBasedOperations = DefaultScheduler.Instance;
            Task = TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
            UI = new SynchronizationContextScheduler(new DispatcherSynchronizationContext(Application.Current.Dispatcher));
        }

        public IScheduler TimeBasedOperations { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler UI { get; }
    }
}
