using BFF.ViewModel.ViewModels.Dialogs;

namespace BFF.View.Wpf.Views.Dialogs
{
    public partial class MainWindowDialogView
    {
        public MainWindowDialogView(IMainWindowDialogViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
