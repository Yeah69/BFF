using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using MrMeeseeks.Extensions;

namespace BFF.View.AttachedBehaviors
{
    public class FrameworkElementClickBehavior : Behavior<FrameworkElement>
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public event EventHandler Click;

        public FrameworkElement Parent => AssociatedObject;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Cursor = Cursors.Hand;

            bool isActive = false;

            Observable
                .FromEventPattern<MouseButtonEventArgs>(AssociatedObject, nameof(UIElement.MouseLeftButtonDown))
                .Subscribe(e => {
                    e.EventArgs.Handled = true;
                    AssociatedObject.CaptureMouse();

                    if (!AssociatedObject.IsMouseCaptured) return;

                    if (e.EventArgs.ButtonState == MouseButtonState.Pressed)
                        isActive = true;
                    else
                        AssociatedObject.ReleaseMouseCapture();
                })
                .AddTo(CompositeDisposable);
            Observable
                .FromEventPattern<MouseButtonEventArgs>(AssociatedObject, nameof(UIElement.MouseLeftButtonUp))
                .Subscribe(e =>
                {
                    e.EventArgs.Handled = true;
                    if (AssociatedObject.IsMouseCaptured)
                        AssociatedObject.ReleaseMouseCapture();
                    if (isActive)
                        Click?.Invoke(this, new EventArgs());
                })
                .AddTo(CompositeDisposable);
            Observable
                .FromEventPattern<MouseEventArgs>(AssociatedObject, nameof(UIElement.LostMouseCapture))
                .Subscribe(e =>
                {
                    if (!Equals(e.EventArgs.OriginalSource))
                        return;
                    isActive = false;
                })
                .AddTo(CompositeDisposable);
            Observable
                .FromEventPattern<MouseEventArgs>(AssociatedObject, nameof(UIElement.MouseMove))
                .Subscribe(e =>
                {
                    if (!AssociatedObject.IsMouseCaptured || Mouse.PrimaryDevice.LeftButton != MouseButtonState.Pressed)
                        return;
                    Point position = Mouse.PrimaryDevice.GetPosition(AssociatedObject);
                    isActive = position.X >= 0.0 && position.X <= AssociatedObject.ActualWidth &&
                               position.Y >= 0.0 && position.Y <= AssociatedObject.ActualHeight;
                    e.EventArgs.Handled = true;
                })
                .AddTo(CompositeDisposable);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            CompositeDisposable.Dispose();
        }
    }
}