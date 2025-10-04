using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace VTFBatcher.Converters;

public class StringEnumConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b && parameter is string s && targetType.IsEnum)
            return Enum.Parse(targetType, s);
        return BindingOperations.DoNothing;
    }
}