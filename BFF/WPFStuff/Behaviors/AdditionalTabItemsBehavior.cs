using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BFF.WPFStuff.Behaviors
{
    public class AdditionalTabItemsBehavior : Behavior<TabControl>
    {
        #region ItemsSource Property

        public IEnumerable<object> ItemsSource
        {
            get { return (IEnumerable<object>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            int blah = 0;
            TabControl tabControl = ((AdditionalTabItemsBehavior)sender).AssociatedObject;
            DataTemplate contenTemplate = ((AdditionalTabItemsBehavior) sender).ContentTemplate;
            if (e.NewValue != null)
            {
                IEnumerable<object> newEnumerable = (IEnumerable<object>)e.NewValue;
                foreach (object obj in newEnumerable)
                {
                    tabControl.Items.Insert(tabControl.Items.Count - 1, new TabItem{Header = obj?.ToString() ?? "NULL", DataContext = obj, Content = contenTemplate.LoadContent()});
                }
            }
        }

        #endregion

        #region ContentTemplate Property

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(null, OnContentTemplateChanged));

        private static void OnContentTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
