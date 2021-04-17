using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BFF.View.Wpf.Views
{
    public partial class NewFlagView
    {
        public event EventHandler? OnAddClicked;

        public NewFlagView()
        {
            InitializeComponent();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnAddClicked?.Invoke(this, new EventArgs());
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FocusMe.Focus();
            Keyboard.Focus(FocusMe);
        }

        private void Flag_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.FindName("Popup") is Popup popup)
            {
                popup.IsOpen = true;
            }
        }
    }
}
