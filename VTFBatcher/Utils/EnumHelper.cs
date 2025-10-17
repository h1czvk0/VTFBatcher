using System;
using System.Collections.Generic;
using System.Linq;

namespace VTFBatcher.Utils;

public static class EnumHelper
{
    // 获取按位枚举的所有已选 Flag（AOT 友好版本，添加 struct 约束）
    public static IEnumerable<T> GetFlags<T>(T value) where T : struct, Enum
    {
        foreach (var flag in Enum.GetValues<T>())
        {
            if (!flag.Equals(default(T)) && value.HasFlag(flag))
                yield return flag;
        }
    }
}
