using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace BFF.Helper
{
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;


        public EnumerationExtension([NotNull] Type enumType)
        {
            enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));

            EnumType = enumType;
        }

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
                    Description = GetDescription(enumValue)
                }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            return EnumType
                .GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() is EnumMemberAttribute enumMemberAttribute
                ? enumMemberAttribute.Value
                : enumValue.ToString();
        }

        public class EnumerationMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }
}
