using Microsoft.Xaml.Behaviors;
using MrMeeseeks.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BFF.View.Wpf.AttachedBehaviors
{
    public class ToolTipCommands : Behavior<ToolTip>
    {
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        public static readonly DependencyProperty OpenedCommandProperty = DependencyProperty.Register(
            nameof(OpenedCommand),
            typeof(ICommand),
            typeof(ToolTipCommands),
            new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ClosedCommandProperty = DependencyProperty.Register(
            nameof(ClosedCommand),
            typeof(ICommand),
            typeof(ToolTipCommands),
            new PropertyMetadata(default(ICommand)));

        

        public ICommand ClosedCommand
        {
            get => (ICommand) GetValue(ClosedCommandProperty);
            set => SetValue(ClosedCommandProperty, value);
        }

        public ICommand OpenedCommand
        {
            get => (ICommand) GetValue(OpenedCommandProperty);
            set => SetValue(OpenedCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            Observable
                .FromEventPattern<RoutedEventArgs>(AssociatedObject, nameof(ToolTip.Opened))
                .Subscribe(e => OpenedCommand?.Execute(e))
                .AddTo(CompositeDisposable);
            Observable
                .FromEventPattern<RoutedEventArgs>(AssociatedObject, nameof(ToolTip.Closed))
                .Subscribe(e => ClosedCommand?.Execute(e))
                .AddTo(CompositeDisposable);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            CompositeDisposable.Dispose();
        }
    }
}