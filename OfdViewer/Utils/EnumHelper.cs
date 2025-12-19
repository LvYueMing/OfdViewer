using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 通用工具：获取枚举的名称/描述
    /// </summary>
    public static class EnumHelper
    {
        // 获取枚举成员名（如 Page/Signature）
        public static string GetEnumName<T>(T enumValue) where T : Enum
        {
            return enumValue.ToString();
        }

        //获取枚举描述（无特性则返回成员名）
        public static string GetEnumDesc<T>(T enumValue) where T : Enum
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }

        // 通过名称字符串转换为枚举值
        public static bool TryParseEnum<T>(string name, out T result) where T : struct, Enum
        {
            // 参数1：字符串名  参数2：是否忽略大小写  参数3：输出结果
            return Enum.TryParse(name, false, out result);
        }


        // 通过名称字符串转换为枚举值
        public static T ParseEnum<T>(string name) where T : struct, Enum
        {
            Enum.TryParse(name, false, out T result);
            return result;
        }

        // 通过描述字符串反向匹配枚举值
        public static bool TryParseByDesc<T>(string desc, out T result) where T : Enum
        {
            result = default;
            // 遍历所有枚举成员，匹配描述
            foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (GetEnumDesc(enumValue) == desc)
                {
                    result = enumValue;
                    return true;
                }
            }
            return false;
        }


        // 通过描述字符串反向匹配枚举值
        public static T ParseByDesc<T>(string desc) where T : Enum
        {
            T result = default;
            // 遍历所有枚举成员，匹配描述
            foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (GetEnumDesc(enumValue) == desc)
                {
                    return enumValue;
                }
            }
            return result;
        }

    }
}
