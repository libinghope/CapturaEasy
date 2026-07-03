using System;
using System.Globalization;
using System.Windows.Data;

namespace Captura
{
    /// <summary>
    /// 枚举值匹配为布尔（用于 RadioButton 绑定枚举）。
    /// ConverterParameter 传具体枚举值，当绑定值等于该参数时返回 true。
    /// </summary>
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.Equals(parameter) ?? false;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? parameter : Binding.DoNothing;
    }
}
