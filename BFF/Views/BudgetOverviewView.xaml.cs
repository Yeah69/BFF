using System;
using System.Windows.Input;
using BFF.AttachedBehaviors;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;

namespace BFF.Views
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

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            void UpdateSlaveCount(int newCount)
            {
                //BudgetEntriesData.DisplayCount = newCount;
                BudgetMonthHeaders.DisplayCount = newCount;
                e.Handled = true;
            }

            switch (e.Key)
            {
                case Key.D1 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(1);
                    break;
                case Key.D2 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(2);
                    break;
                case Key.D3 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(3);
                    break;
                case Key.D4 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(4);
                    break;
                case Key.D5 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(5);
                    break;
                case Key.D6 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(6);
                    break;
                case Key.D7 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(7);
                    break;
                case Key.D8 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(8);
                    break;
                case Key.D9 when Keyboard.Modifiers == ModifierKeys.Control:
                    UpdateSlaveCount(9);
                    break;

            }
        }

        private void OutflowCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent.DataContext is IBudgetEntryViewModel budgetEntry)
                budgetEntry.AssociatedTransElementsViewModel.OpenFlag = true;
        }

        private void AggregatedOutflowCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent.DataContext is IBudgetEntryViewModel budgetEntry)
                budgetEntry.AssociatedAggregatedTransElementsViewModel.OpenFlag = true;
        }

        private void MonthOutflowCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent.DataContext is IBudgetMonthViewModel budgetEntry)
                budgetEntry.AssociatedTransElementsViewModel.OpenFlag = true;
        }

        private void MonthIncomeCell_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent.DataContext is IBudgetMonthViewModel budgetEntry)
                budgetEntry.AssociatedIncomeTransElementsViewModel.OpenFlag = true;
        }

        private void MonthDataHeaderMenu_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent?.ContextMenu != null)
            {
                fecb.Parent.ContextMenu.PlacementTarget = fecb.Parent;
                fecb.Parent.ContextMenu.IsOpen = true;
            }
        }

        private void BudgetEntryMenu_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior fecb && fecb.Parent?.ContextMenu != null)
            {
                fecb.Parent.ContextMenu.PlacementTarget = fecb.Parent;
                fecb.Parent.ContextMenu.IsOpen = true;
            }
        }
    }
}
