using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BFF.MVVM.UserControls
{
    public partial class MasterColumnSlaveColumns
    {

        public static readonly DependencyProperty MasterTemplateProperty = DependencyProperty.Register(
            nameof(MasterTemplate),
            typeof(DataTemplate),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default, OnMasterTemplateChanged));

        public static readonly DependencyProperty SlaveTemplateProperty = DependencyProperty.Register(
            nameof(SlaveTemplate),
            typeof(DataTemplate),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default, OnSlaveTemplateChanged));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default, OnItemsSourceChanged));

        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register(
            nameof(StartIndex),
            typeof(int),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default(int), OnStartIndexChanged));

        public static readonly DependencyProperty SlaveCountProperty = DependencyProperty.Register(
            nameof(SlaveCount),
            typeof(int),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(1, OnSlaveCountChanged));

        public static readonly DependencyProperty MasterContentProperty = DependencyProperty.Register(
            nameof(MasterContent),
            typeof(object),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default, PropertyChangedCallback));

        public object MasterContent
        {
            get => GetValue(MasterContentProperty);
            set => SetValue(MasterContentProperty, value);
        }

        public int SlaveCount
        {
            get => (int) GetValue(SlaveCountProperty);
            set => SetValue(SlaveCountProperty, value);
        }

        public int StartIndex
        {
            get => (int) GetValue(StartIndexProperty);
            set => SetValue(StartIndexProperty, value);
        }

        public IList ItemsSource
        {
            get => (IList) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public DataTemplate SlaveTemplate
        {
            get => (DataTemplate) GetValue(SlaveTemplateProperty);
            set => SetValue(SlaveTemplateProperty, value);
        }

        public DataTemplate MasterTemplate
        {
            get => (DataTemplate) GetValue(MasterTemplateProperty);
            set => SetValue(MasterTemplateProperty, value);
        }

        public MasterColumnSlaveColumns()
        {
            InitializeComponent();

            UniformGrid.Children.Add(new ContentControl
            {
                Content = MasterContent,
                ContentTemplate = MasterTemplate
            });

            if (ItemsSource != null)
            {
                for (int i = 0; i < SlaveCount; i++)
                {
                    UniformGrid.Children.Add(new ContentControl
                    {
                        Content =
                            ItemsSource.Count > StartIndex + i
                                ? ItemsSource[StartIndex + i]
                                : null,
                        ContentTemplate = SlaveTemplate
                    });
                }
            }
        }

        private static void OnMasterTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this &&
                @this.UniformGrid.Children.Count > 0 &&
                @this.UniformGrid.Children[0] is ContentControl masterContentControl)
            {
                masterContentControl.ContentTemplate = args.NewValue as DataTemplate;
            }
        }

        private static void OnSlaveTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this && @this.UniformGrid.Children.Count > 1)
            {
                for (int i = 1; i < @this.UniformGrid.Children.Count; i++)
                {
                    if (@this.UniformGrid.Children[i] is ContentControl contentControl)
                    {
                        contentControl.ContentTemplate = args.NewValue as DataTemplate;
                    }
                }
            }
        }

        private static void OnItemsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this)
            {
                var list = args.NewValue as IList ?? new List<object>();
                int startIndex = @this.StartIndex;

                FillContent(@this, list, startIndex);
            }
        }

        private static void OnStartIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this)
            {
                var list = @this.ItemsSource ?? new List<object>();
                int startIndex = args.NewValue is int i1 ? i1 : 0;

                FillContent(@this, list, startIndex);
            }
        }

        private static void OnSlaveCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this)
            {
                int oldValue = (int)args.OldValue;
                int newValue = (int)args.NewValue;

                if (newValue - oldValue > 0)
                {
                    int diff = newValue - oldValue + 1;
                    for (int i = 0; i < diff; i++)
                    {
                        @this.UniformGrid.Children.Add(
                            new ContentControl
                            {
                                ContentTemplate = @this.SlaveTemplate
                            });
                    }
                }
                else if (newValue - oldValue < 0)
                {
                    int diff = oldValue - newValue;
                    @this.UniformGrid.Children.RemoveRange
                        (@this.UniformGrid.Children.Count - diff, 
                        diff);
                }

                @this.UniformGrid.Columns = newValue + 1;

                FillContent(@this, @this.ItemsSource ?? new List<object>(), @this.StartIndex);
            }
        }

        private static void PropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this && 
                @this.UniformGrid.Children.Count > 0 &&
                @this.UniformGrid.Children[0] is ContentControl masterContentControl)
            {
                masterContentControl.Content = args.NewValue;
            }
        }

        private static void FillContent(MasterColumnSlaveColumns @this, IList list, int startIndex)
        {
            for (int i = 1; i < @this.UniformGrid.Children.Count; i++)
            {
                if (@this.UniformGrid.Children[i] is ContentControl contentControl)
                {
                    int index = startIndex + i;
                    contentControl.Content =
                        list.Count >= index
                            ? list[index - 1]
                            : null;
                }
            }
        }
    }
}
