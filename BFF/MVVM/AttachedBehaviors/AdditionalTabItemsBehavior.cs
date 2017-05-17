using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace BFF.MVVM.AttachedBehaviors
{
    public class AdditionalTabItemsBehavior : Behavior<TabControl>
    {
        private readonly Dictionary<object, TabItem> _objectToTabItem = new Dictionary<object, TabItem>();

        #region ItemsSource Property

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<object>), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TabControl tabControl = ((AdditionalTabItemsBehavior) sender).AssociatedObject;
            DataTemplate headerTemplate = ((AdditionalTabItemsBehavior)sender).HeaderTemplate;
            DataTemplate contentTemplate = ((AdditionalTabItemsBehavior) sender).ContentTemplate;
            int startingIndex = ((AdditionalTabItemsBehavior) sender).StartingIndex;
            Dictionary<object, TabItem> objectToTabItem = ((AdditionalTabItemsBehavior) sender)._objectToTabItem;
            if (e.NewValue != null)
            {
                IEnumerable<object> newEnumerable = (IEnumerable<object>)e.NewValue;
                int i = 0;
                foreach (object obj in newEnumerable)
                {
                    AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + i, objectToTabItem);
                    i++;
                }
                if (newEnumerable is INotifyCollectionChanged)
                {
                    INotifyCollectionChanged notifyCollection = newEnumerable as INotifyCollectionChanged;
                    notifyCollection.CollectionChanged += (o, args) =>
                    {
                        int j;
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                j = 0;
                                foreach (object obj in args.NewItems)
                                {
                                    AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + args.NewStartingIndex + j, objectToTabItem);
                                    j++;
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                foreach (object obj in args.OldItems)
                                {
                                    if(objectToTabItem.ContainsKey(obj))
                                    {
                                        if(tabControl.Items.Contains(obj)) tabControl.Items.Remove(obj);
                                        objectToTabItem.Remove(obj);
                                    }
                                }
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                foreach (object obj in objectToTabItem.Keys)
                                    if (tabControl.Items.Contains(objectToTabItem[obj]))
                                        tabControl.Items.Remove(objectToTabItem[obj]);
                                objectToTabItem.Clear();
                                if (o != null)
                                {
                                    ICollection collection = o as ICollection;
                                    j = 0;
                                    if(collection != null)
                                        foreach (object obj in collection)
                                        {
                                            AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + j, objectToTabItem);
                                            j++;
                                        }
                                }
                                break;
                            case NotifyCollectionChangedAction.Move:
                                throw new NotImplementedException("Creator did not think of this case.");
                            case NotifyCollectionChangedAction.Replace:
                                throw new NotImplementedException("Creator did not think of this case.");
                        }
                        //todo
                    };
                }
            }

        }

        private static void AddTabItem(TabControl tabControl, object obj, DataTemplate headerTemplate, DataTemplate contenTemplate, int index, Dictionary<object, TabItem> objectToTabItem)
        {
            TabItem newTabItem = new TabItem
            {
                DataContext = obj,
                Content = contenTemplate.LoadContent()
            };
            if (headerTemplate != null)
                newTabItem.HeaderTemplate = headerTemplate;
            else
                newTabItem.Header = obj?.ToString() ?? "NULL";
            tabControl.Items.Insert(index, newTabItem);
            if(obj != null) objectToTabItem.Add(obj, newTabItem);
        }

        #endregion

        #region ContentTemplate Property

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(null, OnContentTemplateChanged));

        private static void OnContentTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region HeaderTemplate Property

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(null, OnHeaderTemplateChanged));

        private static void OnHeaderTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region StartingIndex Property

        public int StartingIndex
        {
            get => (int)GetValue(StartingIndexProperty);
            set => SetValue(StartingIndexProperty, value);
        }

        public static readonly DependencyProperty StartingIndexProperty =
            DependencyProperty.Register("StartingIndex", typeof(int), typeof(AdditionalTabItemsBehavior), new UIPropertyMetadata(0, OnStartingIndexChanged));

        private static void OnStartingIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
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
