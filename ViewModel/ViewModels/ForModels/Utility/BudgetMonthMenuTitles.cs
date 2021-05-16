using BFF.ViewModel.Helper;

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
        public BudgetMonthMenuTitles()
        {
        }

        public string EmptyCellsHeader => ""; // ToDo _localizer.Localize("Budgeting_Month_ContextMenu_EmptyCellsHeader");

        public string BudgetLastMonth => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_BudgetLastMonth");

        public string OutflowsLastMonth => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_OutflowsLastMonth");

        public string AvgOutflowLastThreeMonths => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_AvgOutflowsLastThreeMonths");

        public string AvgOutflowsLastYear => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_AvgOutflowsLastYear");

        public string BalanceToZero => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_BalanceToZero");

        public string AllCellsHeader => ""; // ToDo _localizer.Localize("Budgeting_Month_ContextMenu_AllCellsHeader");

        public string Zero => ""; // ToDo _localizer.Localize("Budgeting_ContextMenu_Zero");
    }
}
