using System.Threading.Tasks;
using BFF.ViewModel.ViewModels.Dialogs;

namespace BFF.ViewModel.Helper
{

    public interface IBffChildWindowManager
    {
        Task OpenImportCsvBankStatementDialogAsync(IImportCsvBankStatementViewModel dataContext);
    }
}
