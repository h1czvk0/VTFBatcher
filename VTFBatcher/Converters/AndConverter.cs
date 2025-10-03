using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace VTFBatcher.Converters;

public class AndConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (var v in values)
        {
            if (v is bool b && !b)
                return false;
        }
        return true;
    }
}