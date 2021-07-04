using MrMeeseeks.ResXToViewModelGenerator;

namespace BFF.ViewModel.ViewModels.ForModels.Utility
{
    public interface IBudgetMonthMenuTitles
    {
        string EmptyCellsHeader { get; }
        string BudgetLastMonth { get; }
        string OutflowsLastMonth { get; }
        string AvgOutflowLastThreeMonths { get; }
        string AvgOutflowsLastYear { get; }
        string BalanceToZero { get; }
        string AllCellsHeader { get; }
        string Zero { get; }
    }

    internal class BudgetMonthMenuTitles : IBudgetMonthMenuTitles
    {
        private readonly ICurrentTextsViewModel _currentTextsViewModel;
        
        public BudgetMonthMenuTitles(
            ICurrentTextsViewModel currentTextsViewModel) =>
            _currentTextsViewModel = currentTextsViewModel;

        public string EmptyCellsHeader => _currentTextsViewModel.CurrentTexts.Budgeting_Month_ContextMenu_EmptyCellsHeader;

        public string BudgetLastMonth => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_BudgetLastMonth;

        public string OutflowsLastMonth => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_OutflowsLastMonth;

        public string AvgOutflowLastThreeMonths => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_AvgOutflowsLastThreeMonths;

        public string AvgOutflowsLastYear => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_AvgOutflowsLastYear;

        public string BalanceToZero => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_BalanceToZero;

        public string AllCellsHeader => _currentTextsViewModel.CurrentTexts.Budgeting_Month_ContextMenu_AllCellsHeader;

        public string Zero => _currentTextsViewModel.CurrentTexts.Budgeting_ContextMenu_Zero;
    }
}
