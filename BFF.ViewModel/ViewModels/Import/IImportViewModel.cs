using BFF.Model.ImportExport;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportViewModel
    {
        IImportingConfiguration GenerateConfiguration();
    }
}