using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.Resources
{
    public partial class TransCellTemplates
    {
        public TransCellTemplates()
        {
            InitializeComponent();
        }
        
        private IInputElement _focusedBeforeOpen;

        private void OpenPopup_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.FindName("Popup") is Popup popup)
            {
                _focusedBeforeOpen = element;
                popup.IsOpen = true;
            }
        }

        private void ClosePopup_OnClick(object sender, EventArgs e)
        {
            if (sender is UserControl control && control.Tag is Popup popup)
                popup.IsOpen = false;
        }

        private void Popup_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Popup popup)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        popup.IsOpen = false;
                        e.Handled = true;
                        break;
                }
            }
        }

        private void Popup_OnClosed(object sender, EventArgs e)
        {
            _focusedBeforeOpen?.Focus();
            if (_focusedBeforeOpen != null) Keyboard.Focus(_focusedBeforeOpen);
            _focusedBeforeOpen = null;
        }

        private void OpenFlagPopup_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is IHaveFlagViewModel haveFlagViewModel)
                haveFlagViewModel.NewFlagViewModel.CurrentOwner = haveFlagViewModel;

            OpenPopup_OnClick(sender, e);
        }

        private void OpenPayeePopup_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is IHavePayeeViewModel havePayeeViewModel)
                havePayeeViewModel.NewPayeeViewModel.CurrentOwner = havePayeeViewModel;

            OpenPopup_OnClick(sender, e);
        }

        private void OpenCategoryPopup_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is IHaveCategoryViewModel haveCategoryViewModel)
                haveCategoryViewModel.NewCategoryViewModel.CurrentCategoryOwner = haveCategoryViewModel;

            OpenPopup_OnClick(sender, e);
        }

        private void PayeeSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.FindName("PayeeSelection") is Popup popup)
                popup.IsOpen = true;
        }
    }
}
