using System;
using System.Reactive.Disposables;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.Properties;
using Reactive.Bindings;

namespace BFF.MVVM.Managers
{
    public interface IAccountModuleColumnManager
    {
        IReactiveProperty<bool> ShowFlags { get; }
        IReactiveProperty<bool> ShowCheckNumbers { get; }
    }

    public class AccountModuleColumnManager : IAccountModuleColumnManager, IOncePerApplication, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public AccountModuleColumnManager()
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
