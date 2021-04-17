using System.Reactive.Concurrency;
using Avalonia.Threading;
using BFF.Core.Helper;

namespace BFF.View.Avalonia.Helper
{
    internal class AvaloniaRxSchedulerProvider : IRxSchedulerProvider
    {
        public AvaloniaRxSchedulerProvider()
        {
            TimeBasedOperations = DefaultScheduler.Instance;
            Task = TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
            UI = AvaloniaScheduler.Instance;
        }

        public IScheduler TimeBasedOperations { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler UI { get; }
    }
}
