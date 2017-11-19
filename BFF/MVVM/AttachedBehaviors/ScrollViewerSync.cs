using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using BFF.Helper.Extensions;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.AttachedBehaviors
{
    public class ScrollViewerSync : Behavior<FrameworkElement>
    {
        private static readonly IDictionary<string, IList<ScrollViewer>> GroupNameToScrollViewers = new Dictionary<string, IList<ScrollViewer>>();

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
            nameof(GroupName),
            typeof(string),
            typeof(ScrollViewerSync),
            new PropertyMetadata(default(string), (o, args) =>
            {
                var associatedScrollViewer = (o as ScrollViewerSync)?._associatedScrollViewer;
                if (args.OldValue != null && 
                    GroupNameToScrollViewers.ContainsKey((string)args.OldValue) &&
                    GroupNameToScrollViewers[(string)args.OldValue].Contains(associatedScrollViewer))
                {
                    string key = (string) args.OldValue;
                    GroupNameToScrollViewers[key].Remove(associatedScrollViewer);
                    if (GroupNameToScrollViewers[key].Count == 0)
                        GroupNameToScrollViewers.Remove(key);
                }
                if (args.NewValue != null)
                {
                    string key = (string)args.NewValue;
                    if (!GroupNameToScrollViewers.ContainsKey(key))
                        GroupNameToScrollViewers[key] = new List<ScrollViewer>();
                    if(!GroupNameToScrollViewers[key].Contains(associatedScrollViewer))
                        GroupNameToScrollViewers[key].Add(associatedScrollViewer);
                }
            }));

        public string GroupName
        {
            get => (string) GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private ScrollViewer _associatedScrollViewer = null;

        protected override void OnAttached()
        {
            void OnLoaded(object sender, RoutedEventArgs e)
            {
                _compositeDisposable?.Dispose();
                _compositeDisposable = new CompositeDisposable();

                _associatedScrollViewer = this.AssociatedObject.GetDescendantByType<ScrollViewer>();

                if (!GroupNameToScrollViewers.ContainsKey(GroupName))
                    GroupNameToScrollViewers[GroupName] = new List<ScrollViewer>();
                if (!GroupNameToScrollViewers[GroupName].Contains(_associatedScrollViewer))
                    GroupNameToScrollViewers[GroupName].Add(_associatedScrollViewer);

                Observable.FromEventPattern<ScrollChangedEventArgs>(_associatedScrollViewer, "ScrollChanged")
                    .ObserveOn(Dispatcher)
                    .Subscribe(ep =>
                    {
                        if (GroupName != null
                            && GroupNameToScrollViewers.ContainsKey(GroupName)
                            && GroupNameToScrollViewers[GroupName].Any())
                        {
                            foreach (var scrollViewer in GroupNameToScrollViewers[GroupName].Except(new []{ _associatedScrollViewer}))
                            {
                                scrollViewer?.ScrollToVerticalOffset(_associatedScrollViewer.VerticalOffset);
                            }
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

            this.AssociatedObject.Loaded += OnLoaded;
            this.AssociatedObject.Unloaded += OnUnloaded;

        }
    }
}
