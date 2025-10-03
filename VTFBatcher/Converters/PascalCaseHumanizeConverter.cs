using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace VTFBatcher.Converters;

public class PascalCaseHumanizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return "";
        var str = value.ToString();
        if (string.IsNullOrEmpty(str)) return "";
        return System.Text.RegularExpressions.Regex.Replace(
            str, "(?<!^)([A-Z])", " $1");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}