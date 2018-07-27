using System;
using System.Reactive.Disposables;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.Properties;
using Reactive.Bindings;

namespace BFF.MVVM.Managers
{
    public interface ITransDataGridColumnManager
    {
        IReactiveProperty<bool> ShowFlags { get; }
        IReactiveProperty<bool> ShowCheckNumbers { get; }
    }

    public class TransDataGridColumnManager : ITransDataGridColumnManager, IOncePerApplication, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public TransDataGridColumnManager()
        {
            ShowFlags = new ReactiveProperty<bool>(Settings.Default.ShowFlags, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            ShowFlags.Subscribe(v =>
            {
                Settings.Default.ShowFlags = v;
                Settings.Default.Save();
            }).AddHere(_compositeDisposable);

            ShowCheckNumbers = new ReactiveProperty<bool>(Settings.Default.ShowCheckNumbers, ReactivePropertyMode.DistinctUntilChanged).AddHere(_compositeDisposable);

            ShowCheckNumbers.Subscribe(v =>
            {
                Settings.Default.ShowCheckNumbers = v;
                Settings.Default.Save();
            }).AddHere(_compositeDisposable);
        }

        public IReactiveProperty<bool> ShowFlags { get; }
        public IReactiveProperty<bool> ShowCheckNumbers { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
