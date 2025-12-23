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
        public static bool TryParseEnum<T>(string value, out T result) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                result = default;
                return false;
            }

            // 尝试直接解析（支持数字字符串）
            if (Enum.TryParse(value, false, out result))
            {
                return true;
            }

            // 尝试按数字值解析（兼容 "0"/"1"/"2" 字符串）
            if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(T), intValue))
            {
                result=(T)Enum.ToObject(typeof(T), intValue);
                return true;
            }

            // 尝试按Description特性值解析
            foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (GetEnumDesc(enumValue) == value)
                {
                    result= enumValue;
                    return true;
                }
            }

            result = default;
            return false;
        }


        // 通过名称字符串转换为枚举值
        public static T ParseEnum<T>(string value) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            // 尝试直接解析（支持数字字符串）
            if (Enum.TryParse(value, false, out T result))
            { 
                return result;
            }

            // 尝试按数字值解析（兼容 "0"/"1"/"2" 字符串）
            if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(T), intValue))
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }

            // 尝试按Description特性值解析
            foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (GetEnumDesc(enumValue) == value)
                {
                    return enumValue;
                }
            }

            // 所有解析方式都失败，抛出异常
            throw new ArgumentException($"无法将值 '{value}' 转换为枚举类型 {typeof(T).Name}，支持的解析类型：枚举名称、数字字符串、Description特性值");
        }


    }
}
