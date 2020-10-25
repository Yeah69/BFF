using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BFF.View.UserControls
{
    public partial class UniformList
    {
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(UniformList),
            new PropertyMetadata(default, OnSlaveTemplateChanged));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(UniformList),
            new PropertyMetadata(default, OnItemsSourceChanged));

        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register(
            nameof(StartIndex),
            typeof(int),
            typeof(UniformList),
            new PropertyMetadata(69, OnStartIndexChanged));

        public static readonly DependencyProperty DisplayCountProperty = DependencyProperty.Register(
            nameof(DisplayCount),
            typeof(int),
            typeof(UniformList),
            new PropertyMetadata(1, OnSlaveCountChanged));

        public int DisplayCount
        {
            get => (int) GetValue(DisplayCountProperty);
            set => SetValue(DisplayCountProperty, value);
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

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public UniformList()
        {
            InitializeComponent();

            if (ItemsSource != null)
            {
                for (int i = 0; i < DisplayCount; i++)
                {
                    UniformGrid.Children.Add(new ContentControl
                    {
                        Content =
                            ItemsSource.Count > StartIndex + i
                                ? ItemsSource[StartIndex + i]
                                : null,
                        ContentTemplate = ItemTemplate
                    });
                }
            }
        }

        private static void OnSlaveTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UniformList @this && @this.UniformGrid.Children.Count > 1)
            {
                for (int i = 0; i < @this.UniformGrid.Children.Count; i++)
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
            if (sender is UniformList @this)
            {
                var list = args.NewValue as IList ?? new List<object>();
                int startIndex = @this.StartIndex;

                for (int i = 0; i < @this.UniformGrid.Children.Count; i++)
                {
                    int index = startIndex + i;

                    @this.UniformGrid.Children.Insert(i, new ContentControl
                    {
                        ContentTemplate = @this.ItemTemplate,
                        Content = list.Count > index
                            ? list[index]
                            : null
                    });
                    @this.UniformGrid.Children.RemoveAt(@this.UniformGrid.Children.Count - 1);
                }
            }
        }

        private static void OnStartIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UniformList @this)
            {
                var list = @this.ItemsSource ?? new List<object>();
                int startIndex = args.NewValue is int i1 ? i1 : 0;
                int oldStartIndex = args.OldValue is int i2 ? i2 : 0;

                int diff = oldStartIndex - startIndex;

                if (Math.Abs(diff) >= @this.DisplayCount)
                {
                    for (int i = 0; i < @this.UniformGrid.Children.Count; i++)
                    {
                        int index = startIndex + i;

                        @this.UniformGrid.Children.Insert(i, new ContentControl
                        {
                            ContentTemplate = @this.ItemTemplate,
                            Content = list.Count > index
                                ? list[index]
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
                        @this.UniformGrid.Children.Insert(0, new ContentControl
                        {
                            ContentTemplate = @this.ItemTemplate,
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
                        int index = startIndex + @this.DisplayCount + i;
                        @this.UniformGrid.Children.RemoveAt(0);
                        @this.UniformGrid.Children.Insert(
                            @this.UniformGrid.Children.Count, 
                            new ContentControl
                            {
                                ContentTemplate = @this.ItemTemplate,
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
            if (sender is UniformList @this)
            {
                int oldValue = @this.UniformGrid.Children.Count;
                int newValue = (int)args.NewValue;
                var list = @this.ItemsSource ?? new List<object>();

                @this.UniformGrid.Columns = newValue;

                if (newValue - oldValue > 0)
                {
                    int diff = newValue - oldValue;
                    for (int i = 0; i < diff; i++)
                    {
                        int index = @this.StartIndex + oldValue + i;
                        @this.UniformGrid.Children.Insert(@this.UniformGrid.Children.Count,
                            new ContentControl
                            {
                                ContentTemplate = @this.ItemTemplate,
                                Content = list.Count > index
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
            }
        }
    }
}
