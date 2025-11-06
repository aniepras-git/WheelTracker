using System;
using System.Windows.Markup;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WheelTracker.Helpers
{
    /// <summary>
    /// Markup extension to convert enum to items source for ComboBox, e.g., for WheelStage enum.
    /// </summary>
    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_type)
                .Cast<object>()
                .Select(e => new { Value = e, DisplayName = GetDescription(e) });
        }

        private string GetDescription(object enumValue)
        {
            if (enumValue == null)
            {
                return string.Empty;
            }

            string enumString = enumValue.ToString() ?? string.Empty;

            var fieldInfo = _type.GetField(enumString);

            if (fieldInfo == null)
            {
                return enumString;
            }

            var descriptionAttribute = fieldInfo
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute?.Description ?? enumString;
        }
    }
}