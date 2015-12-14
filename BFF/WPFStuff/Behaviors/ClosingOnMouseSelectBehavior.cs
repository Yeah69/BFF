using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace BFF.WPFStuff.Behaviors
{
    public class ClosingOnMouseSelectBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public bool IsOpen
        {
            get { return (bool)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ClosingOnMouseSelectBehavior), new UIPropertyMetadata(false, OnIsOpenChanged));

        private static void OnIsOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += OnTreeViewMouseLeftButtonDownChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.MouseLeftButtonDown -= OnTreeViewMouseLeftButtonDownChanged;
            }
        }

        private void OnTreeViewMouseLeftButtonDownChanged(object sender, MouseButtonEventArgs e)
        {
            IsOpen = false;
            //SelectedItem = e.NewValue;
        }
    }
}
