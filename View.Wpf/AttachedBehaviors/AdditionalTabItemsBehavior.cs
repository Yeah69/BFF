using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BFF.View.Wpf.AttachedBehaviors
{
    public class AdditionalTabItemsBehavior : Behavior<TabControl>
    {
        private readonly Dictionary<object, TabItem> _objectToTabItem = new();

        #region ItemsSource Property

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable<object>), 
                typeof(AdditionalTabItemsBehavior),
                new UIPropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TabControl tabControl = ((AdditionalTabItemsBehavior) sender).AssociatedObject;
            DataTemplate headerTemplate = ((AdditionalTabItemsBehavior)sender).HeaderTemplate;
            DataTemplate contentTemplate = ((AdditionalTabItemsBehavior) sender).ContentTemplate;
            int startingIndex = ((AdditionalTabItemsBehavior) sender).StartingIndex;
            string isSelectedMemberPath = ((AdditionalTabItemsBehavior) sender).IsSelectedMemberPath;
            Dictionary<object, TabItem> objectToTabItem = ((AdditionalTabItemsBehavior) sender)._objectToTabItem;
            if (e.NewValue is not null)
            {
                IEnumerable<object> newEnumerable = (IEnumerable<object>)e.NewValue;
                int i = 0;
                foreach (object obj in newEnumerable)
                {
                    AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + i, isSelectedMemberPath, objectToTabItem);
                    i++;
                }
                if (newEnumerable is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += (o, args) =>
                    {
                        int j;
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                j = 0;
                                foreach (object obj in args.NewItems?.OfType<object>() ?? Enumerable.Empty<object>())
                                {
                                    AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + args.NewStartingIndex + j, isSelectedMemberPath, objectToTabItem);
                                    j++;
                                }
                                break;
                            case NotifyCollectionChangedAction.Remove:
                                foreach (object obj in args.OldItems?.OfType<object>() ?? Enumerable.Empty<object>())
                                {
                                    if(objectToTabItem.ContainsKey(obj))
                                    {
                                        var tabItem = objectToTabItem[obj];
                                        if (tabControl.Items.Contains(tabItem)) tabControl.Items.Remove(tabItem);
                                        objectToTabItem.Remove(obj);
                                    }
                                }
                                break;
                            case NotifyCollectionChangedAction.Reset:
                                foreach (object obj in objectToTabItem.Keys)
                                    if (tabControl.Items.Contains(objectToTabItem[obj]))
                                        tabControl.Items.Remove(objectToTabItem[obj]);
                                objectToTabItem.Clear();
                                if (o is not null)
                                {
                                    ICollection? collection = o as ICollection;
                                    j = 0;
                                    if(collection is not null)
                                        foreach (object obj in collection.OfType<object>())
                                        {
                                            AddTabItem(tabControl, obj, headerTemplate, contentTemplate, startingIndex + j, isSelectedMemberPath, objectToTabItem);
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

        private static void AddTabItem(
            TabControl tabControl,
            object? obj, 
            DataTemplate headerTemplate, 
            DataTemplate contentTemplate, 
            int index, 
            string isSelectedMemberPath,
            Dictionary<object, TabItem> objectToTabItem)
        {
            TabItem newTabItem = new()
            {
                DataContext = obj,
                Content = contentTemplate.LoadContent()
            };
            if (headerTemplate is not null)
                newTabItem.HeaderTemplate = headerTemplate;
            else
                newTabItem.Header = obj?.ToString() ?? "NULL";
            if(isSelectedMemberPath != default)
                newTabItem.SetBinding(TabItem.IsSelectedProperty, isSelectedMemberPath);
            tabControl.Items.Insert(index, newTabItem);
            if(obj is not null) objectToTabItem.Add(obj, newTabItem);
        }

        #endregion

        #region ContentTemplate Property

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate) GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register(
                nameof(ContentTemplate),
                typeof(DataTemplate), 
                typeof(AdditionalTabItemsBehavior),
                new UIPropertyMetadata(null));
        

        #endregion

        #region HeaderTemplate Property

        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate) GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(HeaderTemplate), 
                typeof(DataTemplate), 
                typeof(AdditionalTabItemsBehavior), 
                new UIPropertyMetadata(null));
        

        #endregion

        #region StartingIndex Property

        public int StartingIndex
        {
            get => (int) GetValue(StartingIndexProperty);
            set => SetValue(StartingIndexProperty, value);
        }

        public static readonly DependencyProperty StartingIndexProperty =
            DependencyProperty.Register(
                nameof(StartingIndex),
                typeof(int), 
                typeof(AdditionalTabItemsBehavior), 
                new UIPropertyMetadata(0));
        

        #endregion

        public static readonly DependencyProperty IsSelectedMemberPathProperty = DependencyProperty.Register(
            nameof(IsSelectedMemberPath),
            typeof(string),
            typeof(AdditionalTabItemsBehavior),
            new PropertyMetadata(default(string)));

        public string IsSelectedMemberPath
        {
            get => (string) GetValue(IsSelectedMemberPathProperty);
            set => SetValue(IsSelectedMemberPathProperty, value);
        }
    }
}
