using BFF.Model.ImportExport;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportViewModel
    {
        IImportConfiguration GenerateConfiguration();
    }
}