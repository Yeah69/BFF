using System;
using System.Windows;

namespace BFF.MVVM.Views
{
    public partial class NewCategoryView
    {
        public event EventHandler OnAddClicked;

        public NewCategoryView()
        {
            InitializeComponent();
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnAddClicked?.Invoke(this, new EventArgs());
        }
    }
}
