using BFF.Model.Contexts;
using BFF.Model.Helper;
using BFF.Model.ImportExport;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BFF.Model.IoC
{
    public interface ICurrentProject
    {
        IObservable<IContext> Current { get; }
        
        void CreateAndSet(ILoadConfiguration loadProjectFileConfiguration);
        void Close();
    }
    
    internal class CurrentProject : ICurrentProject
    {
        private readonly IContextManager _contextManager;
        private readonly IBffSettingsProxy _bffSettingsProxy;
        private readonly SerialDisposable _serialContext = new ();
        private readonly BehaviorSubject<IContext> _current;

        public CurrentProject(
            IContextManager contextManager,
            IBffSettingsProxy bffSettingsProxy,
            CompositeDisposable compositeDisposable)
        {
            _current = new BehaviorSubject<IContext>(contextManager.Empty)
                .CompositeDisposalWith(compositeDisposable);
            _serialContext.CompositeDisposalWith(compositeDisposable);
            _contextManager = contextManager;
            _bffSettingsProxy = bffSettingsProxy;
        }

        public IObservable<IContext> Current => _current.AsObservable();

        public void CreateAndSet(ILoadConfiguration loadProjectFileConfiguration)
        {
            if(_contextManager.CanCreateLoadContext(loadProjectFileConfiguration))
            {
                _current.OnNext(_contextManager.CreateLoadContext(loadProjectFileConfiguration));
                _serialContext.Disposable = _current.Value;
                _bffSettingsProxy.DBLocation = loadProjectFileConfiguration switch
                {
                    IProjectFileAccessConfiguration fileConfiguration => fileConfiguration.Path,
                    _ => ""
                };
            }
        }

        public void Close()
        {
            _current.OnNext(_contextManager.Empty);
            _serialContext.Disposable = _current.Value;
            _bffSettingsProxy.DBLocation = "";
        }
    }
}