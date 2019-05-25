using System;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels;

namespace BFF.ViewModel.Contexts
{
    internal class EmptyProjectContext : ProjectContext, IEmptyProjectContext
    {
        private readonly IDisposable _disposeContext;
        private readonly EmptyTopLevelViewModelComposition _emptyTopLevelViewModelComposition;

        public EmptyProjectContext(
            // parameters
            IDisposable disposeContext,

            // dependencies
            IEmptyCultureManager cultureManger,
            EmptyTopLevelViewModelComposition emptyTopLevelViewModelComposition)
        {
            _disposeContext = disposeContext;
            _emptyTopLevelViewModelComposition = emptyTopLevelViewModelComposition;
            CultureManager = cultureManger;
        }

        public override ICultureManager CultureManager { get; }
        public override TopLevelViewModelCompositionBase LoadProject()
        {
            return _emptyTopLevelViewModelComposition;
        }

        public void Dispose()
        {
            _disposeContext?.Dispose();
        }
    }
}
