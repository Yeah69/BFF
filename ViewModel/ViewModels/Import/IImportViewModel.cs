using BFF.Core.IoC;
using BFF.Model.ImportExport;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportViewModel : ITransient
    {
        IImportConfiguration GenerateConfiguration();
    }
}