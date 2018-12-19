using BFF.Core.Helper;

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
        private readonly ILocalizer _localizer;

        public BudgetMonthMenuTitles(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        public string EmptyCellsHeader => _localizer.Localize("Budgeting_Month_ContextMenu_EmptyCellsHeader");

        public string BudgetLastMonth => _localizer.Localize("Budgeting_ContextMenu_BudgetLastMonth");

        public string OutflowsLastMonth => _localizer.Localize("Budgeting_ContextMenu_OutflowsLastMonth");

        public string AvgOutflowLastThreeMonths => _localizer.Localize("Budgeting_ContextMenu_AvgOutflowsLastThreeMonths");

        public string AvgOutflowsLastYear => _localizer.Localize("Budgeting_ContextMenu_AvgOutflowsLastYear");

        public string BalanceToZero => _localizer.Localize("Budgeting_ContextMenu_BalanceToZero");

        public string AllCellsHeader => _localizer.Localize("Budgeting_Month_ContextMenu_AllCellsHeader");

        public string Zero => _localizer.Localize("Budgeting_ContextMenu_Zero");
    }
}
