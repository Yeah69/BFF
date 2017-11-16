using System.Windows.Input;

namespace BFF.MVVM.Views
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
                BudgetEntriesData.SlaveCount = newCount;
                BudgetMonthHeaders.SlaveCount = newCount;
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
            e.Handled = true;
        }
    }
}
