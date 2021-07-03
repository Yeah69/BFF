using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
namespace BFF.View.Wpf.Helper
{
    public class TextsExtension : MarkupExtension
    {
        private readonly string _path;

        public TextsExtension(string path)
        {
            _path = path;
        }

        public override object ProvideValue(IServiceProvider
            serviceProvider)
        {
            var findName = (Application.Current as App)?.FindResource("TextsHolder");
            if (findName is TextsHolder textsHolder)
                return new Binding($"CurrentTexts.{_path}") { Source = textsHolder.Value, Mode = BindingMode.OneWay};
            throw new InvalidOperationException("TextsHolder should be initialized already");
        }
    }
    
    [MarkupExtensionReturnType(typeof(object))]
    public class BlahExtension : MarkupExtension
    {
        public BlahExtension(PropertyPath path) => Path = path;
        
        [ConstructorArgument("path")]
        public PropertyPath? Path { get; set; } 
     
       public override object? ProvideValue(IServiceProvider serviceProvider)
       {
          var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
          if (pvt == null)
             return null;
     
          var targetObject = pvt.TargetObject as DependencyObject;
          if (targetObject == null)
             return null;
     
          var targetProperty = pvt.TargetProperty as DependencyProperty;
          if (targetProperty == null)
             return null;
          
          var findName = (Application.Current as App)?.FindResource("TextsHolder");
          if (findName is not TextsHolder textsHolder)
            throw new InvalidOperationException("TextsHolder should be initialized already");
     
          var binding = new Binding
          {
             Path = new PropertyPath($"{Path?.Path}"),
             Mode = BindingMode.OneWay,
             Source = textsHolder.Value?.CurrentTexts,
             NotifyOnSourceUpdated = true
          };
     
          var _ = BindingOperations.SetBinding(targetObject, targetProperty, binding);
     
          return targetObject.GetValue(targetProperty);
       }
    }
}