using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using BFF.View.AttachedBehaviors;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MahApps.Metro.Controls;

namespace BFF.View.Resources
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

        private void SymbolNewIcon_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElementClickBehavior behavior 
                && behavior.Parent.FindName("ConvertMenu") is Popup popup)
                popup.IsOpen = true;
        }

        private void FrameworkElementClickBehavior_OnClick(object sender, EventArgs e)
        {
            if (!(sender is FrameworkElementClickBehavior fecb)) return;

            var tryFindParent = fecb.Parent?.TryFindParent<DataGridRow>();
            var parentContextMenu = tryFindParent?.ContextMenu;
            
            if(parentContextMenu != null)
            {
                parentContextMenu.DataContext = tryFindParent.DataContext;
                parentContextMenu.IsOpen = true;
            }
        }
    }
}
