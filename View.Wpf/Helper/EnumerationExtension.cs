using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Markup;

namespace BFF.View.Wpf.Helper
{
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;


        public EnumerationExtension(Type enumType) => _enumType = enumType;

        public Type EnumType
        {
            get => _enumType;
            private set
            {
                if (_enumType == value)
                    return;

                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (enumType.IsEnum == false)
                    throw new ArgumentException("Type must be an Enum.");

                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);

            return (
                from object enumValue in enumValues
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue) ?? String.Empty
                }).ToArray();
        }

        private string? GetDescription(object enumValue)
        {
            return EnumType
                .GetField(enumValue.ToString() ?? String.Empty)
                ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() is EnumMemberAttribute enumMemberAttribute
                ? enumMemberAttribute.Value
                : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; } = String.Empty;
            public object Value { get; set; } = new object();
        }
    }
}
