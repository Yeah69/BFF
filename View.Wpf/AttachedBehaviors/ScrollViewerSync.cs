﻿using BFF.View.Wpf.Extensions;
using Microsoft.Xaml.Behaviors;
using MrMeeseeks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BFF.View.Wpf.AttachedBehaviors
{
    public class ScrollViewerSync : Behavior<FrameworkElement>
    {
        private static readonly IDictionary<string, IList<ScrollViewer?>> GroupNameToScrollViewers = new Dictionary<string, IList<ScrollViewer?>>();
        private static readonly IDictionary<string, double> GroupNameToScrollPosition = new Dictionary<string, double>();

        private static readonly IScheduler
            DispatcherScheduler = new SynchronizationContextScheduler(new DispatcherSynchronizationContext(Application.Current.Dispatcher));

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(ScrollViewerSync),
            new PropertyMetadata(default(string), (o, args) =>
            {
                var associatedScrollViewer = (o as ScrollViewerSync)?._associatedScrollViewer;
                if (args.OldValue is not null && 
                    GroupNameToScrollViewers.ContainsKey((string)args.OldValue) &&
                    GroupNameToScrollViewers[(string)args.OldValue].Contains(associatedScrollViewer))
                {
                    string key = (string) args.OldValue;
                    GroupNameToScrollViewers[key].Remove(associatedScrollViewer);
                    if (GroupNameToScrollViewers[key].Count == 0)
                        GroupNameToScrollViewers.Remove(key);
                }
                if (args.NewValue is not null)
                {
                    string key = (string)args.NewValue;
                    if (!GroupNameToScrollViewers.ContainsKey(key))
                        GroupNameToScrollViewers[key] = new List<ScrollViewer?>();
                    if(!GroupNameToScrollViewers[key].Contains(associatedScrollViewer))
                        GroupNameToScrollViewers[key].Add(associatedScrollViewer);
                }
            }));

        public string GroupName
        {
            get => (string) GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private CompositeDisposable? _compositeDisposable = new();

        private ScrollViewer? _associatedScrollViewer;

        protected override void OnAttached()
        {
            void OnLoaded(object sender, RoutedEventArgs e)
            {
                _compositeDisposable?.Dispose();
                _compositeDisposable = new CompositeDisposable();

                _associatedScrollViewer = AssociatedObject.GetDescendantByType<ScrollViewer>();

                if (_associatedScrollViewer is null) return;

                if (!GroupNameToScrollViewers.ContainsKey(GroupName))
                    GroupNameToScrollViewers[GroupName] = new List<ScrollViewer?>();
                if (!GroupNameToScrollPosition.ContainsKey(GroupName))
                    GroupNameToScrollPosition[GroupName] = 0;
                if (!GroupNameToScrollViewers[GroupName].Contains(_associatedScrollViewer))
                    GroupNameToScrollViewers[GroupName].Add(_associatedScrollViewer);
                _associatedScrollViewer.ScrollToVerticalOffset(GroupNameToScrollPosition[GroupName]);

                Observable.FromEventPattern<ScrollChangedEventArgs>(_associatedScrollViewer, nameof(ScrollViewer.ScrollChanged))
                    .ObserveOn(DispatcherScheduler)
                    .Subscribe(ep =>
                    {
                        if (GroupName is not null
                            && GroupNameToScrollViewers.ContainsKey(GroupName)
                            && GroupNameToScrollViewers[GroupName].Any())
                        {
                            foreach (var scrollViewer in GroupNameToScrollViewers[GroupName].Except(new []{ _associatedScrollViewer}))
                            {
                                scrollViewer?.ScrollToVerticalOffset(_associatedScrollViewer.VerticalOffset);
                            }

                            GroupNameToScrollPosition[GroupName] = _associatedScrollViewer.VerticalOffset;
                        }
                    })
                    .AddTo(_compositeDisposable);
            }

            void OnUnloaded(object sender, RoutedEventArgs e)
            {
                GroupNameToScrollViewers[GroupName].Remove(_associatedScrollViewer);
                if (GroupNameToScrollViewers[GroupName].Count == 0)
                    GroupNameToScrollViewers.Remove(GroupName);
                _compositeDisposable?.Dispose();
                _compositeDisposable = null;
                _associatedScrollViewer = null;
            }

            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;

        }
    }
}
