using System.Threading.Tasks;

namespace BFF.ViewModel.Helper
{
    public enum BffMessageDialogStyle
    {
        Affirmative,
        AffirmativeAndNegative
    }
    public enum BffMessageDialogResult
    {
        Affirmative,
        Negative
    }

    public interface IBffDialogCoordinator
    {
        Task<BffMessageDialogResult> ShowMessageAsync(
            string title,
            string message,
            BffMessageDialogStyle style = BffMessageDialogStyle.Affirmative);
    }

    public interface IMainBffDialogCoordinator : IBffDialogCoordinator
    {

    }
}
