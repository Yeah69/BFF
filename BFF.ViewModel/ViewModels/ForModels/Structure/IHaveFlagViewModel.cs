namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface IHaveFlagViewModel
    {
        IFlagViewModel Flag { get; set; }

        INewFlagViewModel NewFlagViewModel { get; }
    }
}
