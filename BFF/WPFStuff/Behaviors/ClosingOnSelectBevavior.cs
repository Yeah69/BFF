using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BFF.WPFStuff.Behaviors
{
    public class ClosingOnSelectBevavior : BindableSelectedItemBehavior
    {
        #region SelectedItem Property

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ClosingOnSelectBevavior), new UIPropertyMetadata(false, OnIsOpenChanged));

        private static void OnIsOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        protected override void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(IsOpen)
                base.OnTreeViewSelectedItemChanged(sender, e);
            IsOpen = false;
        }
    }
}
