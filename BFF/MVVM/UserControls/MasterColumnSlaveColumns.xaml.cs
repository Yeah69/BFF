using System;
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
            new PropertyMetadata(69, OnStartIndexChanged));

        public static readonly DependencyProperty SlaveCountProperty = DependencyProperty.Register(
            nameof(SlaveCount),
            typeof(int),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(1, OnSlaveCountChanged));

        public static readonly DependencyProperty MasterContentProperty = DependencyProperty.Register(
            nameof(MasterContent),
            typeof(object),
            typeof(MasterColumnSlaveColumns),
            new PropertyMetadata(default, OnMasterContentChanged));

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

                for (int i = 1; i < @this.UniformGrid.Children.Count; i++)
                {
                    int index = startIndex + i;

                    @this.UniformGrid.Children.Insert(i, new ContentControl
                    {
                        ContentTemplate = @this.SlaveTemplate,
                        Content = list.Count >= index
                            ? list[index - 1]
                            : null
                    });
                    @this.UniformGrid.Children.RemoveAt(@this.UniformGrid.Children.Count - 1);
                }
            }
        }

        private static void OnStartIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this)
            {
                var list = @this.ItemsSource ?? new List<object>();
                int startIndex = args.NewValue is int i1 ? i1 : 0;
                int oldStartIndex = args.OldValue is int i2 ? i2 : 0;

                int diff = oldStartIndex - startIndex;

                if (Math.Abs(diff) >= @this.SlaveCount)
                {
                    for (int i = 1; i < @this.UniformGrid.Children.Count; i++)
                    {
                        int index = startIndex + i;

                        @this.UniformGrid.Children.Insert(i, new ContentControl
                        {
                            ContentTemplate = @this.SlaveTemplate,
                            Content = list.Count > index
                                ? list[index - 1]
                                : null
                        });
                        @this.UniformGrid.Children.RemoveAt(@this.UniformGrid.Children.Count - 1);
                    }
                }
                else if (diff > 0)
                {
                    for (int i = diff - 1 ; i >= 0; i--)
                    {
                        int index = startIndex;
                        @this.UniformGrid.Children.RemoveAt(@this.UniformGrid.Children.Count - 1);
                        @this.UniformGrid.Children.Insert(1, new ContentControl
                        {
                            ContentTemplate = @this.SlaveTemplate,
                            Content = list.Count > index
                                ? list[index]
                                : null
                        });
                    }
                }
                else if (diff < 0)
                {
                    for (int i = diff; i < 0; i++)
                    {
                        int index = startIndex + @this.SlaveCount + i;
                        @this.UniformGrid.Children.RemoveAt(1);
                        @this.UniformGrid.Children.Insert(
                            @this.UniformGrid.Children.Count, 
                            new ContentControl
                            {
                                ContentTemplate = @this.SlaveTemplate,
                                Content = list.Count > index
                                    ? list[index]
                                    : null
                            });
                    }
                }
            }
        }

        private static void OnSlaveCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this)
            {
                int oldValue = @this.UniformGrid.Children.Count - 1;
                int newValue = (int)args.NewValue;
                var list = @this.ItemsSource ?? new List<object>();

                @this.UniformGrid.Columns = newValue + 1;

                if (newValue - oldValue > 0)
                {
                    int diff = newValue - oldValue;
                    for (int i = 0; i < diff; i++)
                    {
                        int index = @this.StartIndex + oldValue + i;
                        @this.UniformGrid.Children.Insert(@this.UniformGrid.Children.Count,
                            new ContentControl
                            {
                                ContentTemplate = @this.SlaveTemplate,
                                Content = list.Count >= index
                                    ? list[index]
                                    : null
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
            }
        }

        private static void OnMasterContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is MasterColumnSlaveColumns @this && 
                @this.UniformGrid.Children.Count > 0 &&
                @this.UniformGrid.Children[0] is ContentControl masterContentControl)
            {
                masterContentControl.Content = args.NewValue;
            }
        }
    }
}
