using BFF.Model.ImportExport;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IExportViewModel
    {
        IExportingConfiguration GenerateConfiguration();
    }
}