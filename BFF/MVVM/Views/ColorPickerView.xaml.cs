using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        public static readonly DependencyProperty RedProperty = DependencyProperty.Register(
            nameof(Red),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty GreenProperty = DependencyProperty.Register(
            nameof(Green),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty BlueProperty = DependencyProperty.Register(
            nameof(Blue),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register(
            nameof(Alpha),
            typeof(byte),
            typeof(ColorPickerView),
            new PropertyMetadata(default(byte), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color),
            typeof(Color),
            typeof(ColorPickerView),
            new PropertyMetadata(default(Color), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
            nameof(Hue),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register(
            nameof(Saturation),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(float),
            typeof(ColorPickerView),
            new PropertyMetadata(default(float), (o, args) => (o as ColorPickerView)?.UpdateColorProperties()));

        private static readonly DependencyPropertyKey BrushPropertyKey
            = DependencyProperty.RegisterReadOnly(
                nameof(Brush),
                typeof(SolidColorBrush),
                typeof(ColorPickerView),
                new FrameworkPropertyMetadata(
                    default(SolidColorBrush),
                    FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty BrushProperty
            = BrushPropertyKey.DependencyProperty;

        

        public SolidColorBrush Brush
        {
            get => (SolidColorBrush) GetValue(BrushProperty);
            protected set => SetValue(BrushPropertyKey, value);
        }

        public float Value
        {
            get => (float) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public float Saturation
        {
            get => (float) GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public float Hue
        {
            get => (float) GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public byte Alpha
        {
            get => (byte) GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public byte Blue
        {
            get => (byte) GetValue(BlueProperty);
            set => SetValue(BlueProperty, value);
        }

        public byte Green
        {
            get => (byte) GetValue(GreenProperty);
            set => SetValue(GreenProperty, value);
        }

        public byte Red
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
            }.SetDependencyProperty(Panel.ZIndexProperty, -2);

        private readonly Ellipse _swatchPointerInner =
            new Ellipse
            {
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Height = 8,
                Width = 8
            }.SetDependencyProperty(Panel.ZIndexProperty, -1);

        public ColorPickerView()
        {
            InitializeComponent();

            _swatchPointerOuter.Stroke = FindResource("WhiteBrush") as Brush;
            _swatchPointerInner.Stroke = FindResource("AccentColorBrush") as Brush;

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
                }.SetDependencyProperty(Panel.ZIndexProperty, -3));
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

        private void UpdateColorProperties()
        {
            (float Hue, float Saturation, float Value) ColorToHsv(Color color)
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
            
            Red = Color.R;
            Green = Color.G;
            Blue = Color.B;
            Alpha = Color.A;
            (Hue, Saturation, Value) = ColorToHsv(Color);
            Brush = new SolidColorBrush(Color);

            HuePointerOuter.SetValue(Canvas.TopProperty, (double) (Hue - 6));
            HuePointerInner.SetValue(Canvas.TopProperty, (double) (Hue - 4));

            Swatch.Children.Clear();

            Swatch.Children.Add(_swatchPointerOuter);
            Swatch.Children.Add(_swatchPointerInner);

            //<Rectangle x:Name="HuePointerOuter" Canvas.Left="1" />
            //<Rectangle x:Name="HuePointerInner" Canvas.Left="3" />
            _swatchPointerOuter
                .SetDependencyProperty(Canvas.TopProperty, (double) (360 - Value * 360 - 6))
                .SetDependencyProperty(Canvas.LeftProperty, (double) (Saturation * 360 - 6));
            _swatchPointerInner
                .SetDependencyProperty(Canvas.TopProperty, (double) (360 - Value * 360 - 4))
                .SetDependencyProperty(Canvas.LeftProperty, (double) (Saturation * 360 - 4));

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
                }.SetDependencyProperty(Panel.ZIndexProperty, -3));
            }
        }
    }
}
