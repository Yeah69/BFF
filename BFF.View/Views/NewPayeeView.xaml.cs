using System;
using System.Windows;
using System.Windows.Input;

namespace BFF.View.Views
{
    public partial class NewPayeeView
    {
        public event EventHandler OnAddClicked;

        public NewPayeeView()
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
    }
}
