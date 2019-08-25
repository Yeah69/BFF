using BFF.Core.Persistence;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportViewModel
    {
        IImportingConfiguration GenerateConfiguration();
    }
}