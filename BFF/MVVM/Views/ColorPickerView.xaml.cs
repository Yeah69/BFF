using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BFF.Helper.Extensions;

namespace BFF.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ColorPickerView.xaml
    /// </summary>
    public partial class ColorPickerView
    {
        private static readonly DependencyProperty RedProperty = DependencyProperty.Register(
            nameof(Red),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte)));

        private static readonly DependencyProperty GreenProperty = DependencyProperty.Register(
            nameof(Green),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte)));

        private static readonly DependencyProperty BlueProperty = DependencyProperty.Register(
            nameof(Blue),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte)));

        private static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(
            nameof(Alpha),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte)));

        private static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(ColorPickerView),
            new PropertyMetadata(default(Color)));

        private static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float)));

        private static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float)));

        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float)));

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(ColorPickerView),
            new PropertyMetadata(default(SolidColorBrush), (o, args) =>
            {
                var colorPickerView = o as ColorPickerView;
                if (colorPickerView != null)
                {
                    colorPickerView.Color = colorPickerView.Brush?.Color ?? Colors.Transparent;
                    colorPickerView.UpdateColorProperties();
                    colorPickerView.ResetHuePointer();
                    colorPickerView.RedrawSwatch();
                    colorPickerView.ResetSwatchPointer();
                    colorPickerView.RedrawAlphaScale();
                    colorPickerView.ResetAlphaPointer();
                }
            }));

        

        public SolidColorBrush Brush
        {
            get => (SolidColorBrush) GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        private float Value
        {
            get => (float) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private float Saturation
        {
            get => (float) GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        private float Hue
        {
            get => (float) GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        private Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private byte Alpha
        {
            get => (byte) GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        private byte Blue
        {
            get => (byte) GetValue(BlueProperty);
            set => SetValue(BlueProperty, value);
        }

        private byte Green
        {
            get => (byte) GetValue(GreenProperty);
            set => SetValue(GreenProperty, value);
        }

        private byte Red
        {
            get => (byte) GetValue(RedProperty);
            set => SetValue(RedProperty, value);
        }

        private readonly Ellipse _swatchPointerOuter =
            new Ellipse
            {
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Height = 12,
                Width = 12
            }.SetDependencyProperty(Panel.ZIndexProperty, 2);

        private readonly Ellipse _swatchPointerInner =
            new Ellipse
            {
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Height = 8,
                Width = 8
            }.SetDependencyProperty(Panel.ZIndexProperty, 3);

        private readonly Rectangle _alphaPointerOuter =
            new Rectangle
            {
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Height = 18,
                Width = 12
            }
                .SetDependencyProperty(Panel.ZIndexProperty, 2)
                .SetDependencyProperty(Canvas.TopProperty, 1.0);

        private readonly Rectangle _alphaPointerInner =
            new Rectangle
            {
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Height = 14,
                Width = 8
            }
                .SetDependencyProperty(Panel.ZIndexProperty, 3)
                .SetDependencyProperty(Canvas.TopProperty, 3.0);

        private readonly SerialDisposable _adjustmentOperation  = new SerialDisposable();

        public ColorPickerView()
        {
            InitializeComponent();

            _swatchPointerOuter.SetResourceReference(Shape.StrokeProperty, "WhiteBrush");
            _swatchPointerInner.SetResourceReference(Shape.StrokeProperty, "AccentColorBrush");

            _alphaPointerOuter.SetResourceReference(Shape.StrokeProperty, "WhiteBrush");
            _alphaPointerInner.SetResourceReference(Shape.StrokeProperty, "AccentColorBrush");

            for (int i = 0; i < 360; i++)
            {
                Color hueColor = HsvToColor(i, 1.0f, 1.0f);
                HueScale.Children.Add(new Line
                {
                    Stroke = new SolidColorBrush(hueColor),
                    X1 = 0,
                    Y1 = i,
                    X2 = 20,
                    Y2 = i
                }.SetDependencyProperty(Panel.ZIndexProperty, 1));
            }
        }


        private Color HsvToColor(float h, float s, float v)
        {
            float newH = (float) Math.Floor(h / 60.0f);

            float f = h / 60.0f - newH;

            float p = v * (1.0f - s);

            float q = v * (1.0f - s * f);

            float t = v * (1.0f - s * (1.0f - f));

            (float r, float g, float b) result = (0.0f, 0.0f, 0.0f);

            switch (newH)
            {
                case 0.0f:
                case 6.0f:
                    result = (v, t, p);
                    break;
                case 1.0f:
                    result = (q, v, p);
                    break;
                case 2.0f:
                    result = (p, v, t);
                    break;
                case 3.0f:
                    result = (p, q, v);
                    break;
                case 4.0f:
                    result = (t, p, v);
                    break;
                case 5.0f:
                    result = (v, p, q);
                    break;
            }

            return Color.FromRgb(
                (byte) (result.r * 255.0f),
                (byte) (result.g * 255.0f),
                (byte) (result.b * 255.0f));
        }

        private (float Hue, float Saturation, float Value) ColorToHsv(Color color)
        {
            float r = color.R / 255.0f;
            float g = color.G / 255.0f;
            float b = color.B / 255.0f;

            float min = new[] { r, g, b }.Min();
            float max = new[] { r, g, b }.Max();

            float diff = max - min;

            float h = 0.0f;

            if (diff != 0.0f)
            {
                if (max == r)
                    h = 60.0f * ((g - b) / diff);
                else if (max == g)
                    h = 60.0f * ((b - r) / diff + 2.0f);
                else if (max == b)
                    h = 60.0f * ((r - g) / diff + 4.0f);
            }

            float v = max;

            float s = max == 0.0f ? 0.0f : diff / max;

            return (h, s, v);
        }

        private void UpdateColorProperties()
        {
            UpdateRgbProperties();
            UpdateHsvProperties();
        }

        private void UpdateRgbProperties()
        {
            Red = Color.R;
            Green = Color.G;
            Blue = Color.B;
            Alpha = Color.A;
        }

        private void UpdateHsvProperties()
        {
            (Hue, Saturation, Value) = ColorToHsv(Color);
        }

        private void ResetHuePointer()
        {
            HuePointerOuter.SetValue(Canvas.TopProperty, (double)(Hue - 6));
            HuePointerInner.SetValue(Canvas.TopProperty, (double)(Hue - 4));
        }

        private void ResetAlphaPointer()
        {
            _alphaPointerOuter
                .SetDependencyProperty(Canvas.LeftProperty, Alpha / 255.0 * 360.0 - 6);
            _alphaPointerInner
                .SetDependencyProperty(Canvas.LeftProperty, Alpha / 255.0 * 360.0 - 4);
        }

        private void ResetSwatchPointer()
        {
            _swatchPointerOuter
                .SetDependencyProperty(Canvas.TopProperty, (double)(360 - Value * 360 - 6))
                .SetDependencyProperty(Canvas.LeftProperty, (double)(Saturation * 360 - 6));
            _swatchPointerInner
                .SetDependencyProperty(Canvas.TopProperty, (double)(360 - Value * 360 - 4))
                .SetDependencyProperty(Canvas.LeftProperty, (double)(Saturation * 360 - 4));
        }

        private void RedrawSwatch()
        {
            Swatch.Children.Clear();

            Swatch.Children.Add(_swatchPointerOuter);
            Swatch.Children.Add(_swatchPointerInner);

            

            for (int i = 0; i < 360; i++)
            {
                Color currentColor = HsvToColor(Hue, 1.0f, 1.0f - i / 360.0f);
                Color currentSatLessColor = HsvToColor(Hue, 0.0f, 1.0f - i / 360.0f);
                Swatch.Children.Add(new Line
                {
                    Stroke = new LinearGradientBrush(currentSatLessColor, currentColor, 0.0),
                    StrokeThickness = 1.0,
                    X1 = 0.0,
                    Y1 = i,
                    X2 = 360,
                    Y2 = i
                }.SetDependencyProperty(Panel.ZIndexProperty, 1));
            }
        }

        private void RedrawAlphaScale()
        {
            AlphaScale.Children.Clear();

            AlphaScale.Children.Add(_alphaPointerOuter);
            AlphaScale.Children.Add(_alphaPointerInner);
            
            for (int i = 0; i < 360; i++)
            {
                Color alphaScaleColor = Color.FromArgb((byte) (i / 360.0 * 255.0), Color.R, Color.G, Color.B);
                AlphaScale.Children.Add(new Line
                {
                    Stroke = new SolidColorBrush(alphaScaleColor),
                    Y1 = 0,
                    X1 = i,
                    Y2 = 20,
                    X2 = i
                }.SetDependencyProperty(Panel.ZIndexProperty, 1));
            }
        }

        private void Swatch_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            void OnSwatchChange(Point p)
            {
                Saturation = (float)p.X / 360.0f;
                Value = 1.0f - (float)p.Y / 360.0f;
                Color = HsvToColor(Hue, Saturation, Value);
                Color = Color.FromArgb(Alpha, Color.R, Color.G, Color.B);
                UpdateRgbProperties();
                RedrawAlphaScale();
                ResetAlphaPointer();
                ResetSwatchPointer();
            }

            var position = e.GetPosition(sender as IInputElement);
            OnSwatchChange(position);
            _adjustmentOperation.Disposable =
                ObserveMouseMoveUntilMouseUp()
                    .Subscribe(
                        _ =>
                        {
                            var point = Mouse
                                .GetPosition(Swatch)
                                .Coerce(0, Swatch.Width, 0, Swatch.Height);
                            OnSwatchChange(point);
                        },
                        () => _adjustmentOperation.Disposable = Disposable.Empty);
            
            e.Handled = true;
        }

        private void HueScale_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            void OnHueScaleChange(Point p)
            {
                Hue = (float)p.Y;
                Color = HsvToColor(Hue, Saturation, Value);
                Color = Color.FromArgb(Alpha, Color.R, Color.G, Color.B);
                UpdateRgbProperties();
                ResetHuePointer();
                RedrawSwatch();
                ResetSwatchPointer();
                RedrawAlphaScale();
            }

            var position = e.GetPosition(HueScale);
            OnHueScaleChange(position);
            _adjustmentOperation.Disposable =
                ObserveMouseMoveUntilMouseUp()
                    .Subscribe(
                        _ =>
                        {
                            var point = Mouse
                                .GetPosition(HueScale)
                                .Coerce(0, 0, 0, HueScale.Height);
                            OnHueScaleChange(point);
                        },
                        () => _adjustmentOperation.Disposable = Disposable.Empty);

            e.Handled = true;
        }

        private void AlphaScale_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            void OnAlphaScaleChange(Point p)
            {
                Alpha = (byte)(p.X / 360.0f * 255.0);
                Color = Color.FromArgb(Alpha, Red, Green, Blue);
                ResetAlphaPointer();
            }

            var position = e.GetPosition(AlphaScale);
            OnAlphaScaleChange(position);
            _adjustmentOperation.Disposable =
                ObserveMouseMoveUntilMouseUp()
                    .Subscribe(
                        _ =>
                        {
                            var point = Mouse
                                .GetPosition(AlphaScale)
                                .Coerce(0, AlphaScale.Width, 0, 0);
                            OnAlphaScaleChange(point);
                        },
                        () => _adjustmentOperation.Disposable = Disposable.Empty);

            e.Handled = true;
        }

        private IObservable<EventPattern<MouseEventArgs>> ObserveMouseMoveUntilMouseUp() =>
            Observable.FromEventPattern<MouseEventArgs>(Application.Current.MainWindow, "MouseMove")
                .SubscribeOn(NewThreadScheduler.Default)
                .TakeUntil(Observable.FromEventPattern<MouseButtonEventArgs>(Application.Current.MainWindow, "MouseUp"))
                .Sample(TimeSpan.FromMilliseconds(20))
                .ObserveOn(new DispatcherScheduler(Application.Current.Dispatcher));

        private void Preview_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Preview_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Brush = new SolidColorBrush(Color);
            GetBindingExpression(BrushProperty)?.UpdateSource();
        }
    }
}
