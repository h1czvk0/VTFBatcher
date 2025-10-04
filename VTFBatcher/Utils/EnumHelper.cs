using System;
using System.Collections.Generic;

namespace VTFBatcher.Utils;

public static class EnumHelper
{
    public static List<T> GetFlags<T>(T input) where T : Enum
    {
        var result = new List<T>();
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if (input.HasFlag(value) && !value.Equals(default(T)))
            {
                result.Add(value);
            }
        }
        return result;
    }
}