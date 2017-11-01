using System;
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
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            nameof(VerticalOffset),
            typeof(double),
            typeof(ScrollViewerSync),
            new PropertyMetadata(default(double), (o, args) => (o as ScrollViewerSync)?._associatedScrollViewer?.ScrollToVerticalOffset((double)args.NewValue)));

        public double VerticalOffset
        {
            get => (double)this.GetValue(VerticalOffsetProperty);
            set => this.SetValue(VerticalOffsetProperty, value);
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
                Observable.FromEventPattern<ScrollChangedEventArgs>(_associatedScrollViewer, "ScrollChanged")
                    .Subscribe(ep => this.VerticalOffset = ep.EventArgs.VerticalOffset)
                    .AddTo(_compositeDisposable);
            }

            void OnUnloaded(object sender, RoutedEventArgs e)
            {
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
