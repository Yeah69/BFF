using BFF.View.Wpf.AttachedBehaviors;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BFF.View.Wpf.Views
{
    /// <summary>
    /// Interaction logic for BudgetOverviewView.xaml
    /// </summary>
    public partial class BudgetOverviewView
    {
        public BudgetOverviewView()
        {
            InitializeComponent();
        }

        private void OutflowCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent.DataContext is IBudgetEntryViewModel budgetEntry
                && this.DataContext is IBudgetOverviewViewModel budgetOverviewViewModel)
            {
                if (budgetOverviewViewModel.Table.ShowAggregates
                    && budgetEntry.AssociatedAggregatedTransElementsViewModel is {} associatedAggregatedTransElementsViewModel)
                    associatedAggregatedTransElementsViewModel.OpenFlag = true;
                else if (budgetEntry.AssociatedTransElementsViewModel is {} associatedTransElementsViewModel)
                    associatedTransElementsViewModel.OpenFlag = true;
            }
        }

        private void MonthOutflowCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb 
                && fecb.Parent.DataContext is IBudgetMonthViewModel budgetEntry
                && budgetEntry.AssociatedTransElementsViewModel is {} associatedTransElementsViewModel)
                associatedTransElementsViewModel.OpenFlag = true;
        }

        private void MonthIncomeCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb 
                && fecb.Parent.DataContext is IBudgetMonthViewModel budgetEntry
                && budgetEntry.AssociatedIncomeTransElementsViewModel is {} associatedIncomeTransElementsViewModel)
                associatedIncomeTransElementsViewModel.OpenFlag = true;
        }

        private void MonthDataHeaderMenu_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent?.ContextMenu is not null)
            {
                fecb.Parent.ContextMenu.PlacementTarget = fecb.Parent;
                fecb.Parent.ContextMenu.IsOpen = true;
            }
        }

        private void BudgetEntryMenu_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent?.ContextMenu is not null)
            {
                fecb.Parent.ContextMenu.PlacementTarget = fecb.Parent;
                fecb.Parent.ContextMenu.IsOpen = true;
            }
        }
    }

    public class BudgetMonthItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Content { get; set; } = Empty;

        public DataTemplate Placeholder { get; set; } = Empty;

        private static DataTemplate Empty { get; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                BudgetMonthViewModel _ => Content,
                BudgetMonthViewModelPlaceholder _ => Placeholder,
                _ => Empty
            };
        }
    }
}
