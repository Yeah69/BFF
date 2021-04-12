using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels;

namespace BFF.ViewModel.Contexts
{
    internal class EmptyContextViewModel : ContextViewModel, IEmptyContextViewModel
    {
        private readonly EmptyTopLevelViewModelComposition _emptyTopLevelViewModelComposition;

        public EmptyContextViewModel(
            IEmptyCultureManager cultureManger,
            EmptyTopLevelViewModelComposition emptyTopLevelViewModelComposition)
        {
            _emptyTopLevelViewModelComposition = emptyTopLevelViewModelComposition;
            CultureManager = cultureManger;
        }

        public override ICultureManager CultureManager { get; }
        public override TopLevelViewModelCompositionBase LoadProject() => _emptyTopLevelViewModelComposition;
    }
}
