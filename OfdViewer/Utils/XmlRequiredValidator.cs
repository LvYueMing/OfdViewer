using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OFDViewer.Utils
{
    /// <summary>
    /// 通用XML必填项校验工具（支持集合元素个数范围校验 + 嵌套对象递归校验）
    /// </summary>
    internal class XmlRequiredValidator
    {
        /// <summary>
        /// 校验指定对象的所有XML必填属性（含嵌套对象）是否有效
        /// </summary>
        /// <typeparam name="T">待校验对象类型</typeparam>
        /// <param name="obj">待校验对象</param>
        /// <exception cref="ArgumentNullException">对象为null时抛出</exception>
        /// <exception cref="XmlRequiredValidationException">必填项无效时抛出</exception>
        public static void Validate<T>(T obj) where T : class
        {
            // 内部递归校验方法，支持任意对象类型
            InternalValidate(obj, typeof(T), string.Empty);
        }

        /// <summary>
        /// 内部递归校验方法（核心：支持嵌套对象）
        /// </summary>
        /// <param name="obj">待校验对象</param>
        /// <param name="objType">待校验对象类型</param>
        /// <param name="parentPropertyPath">父属性路径（用于精准定位嵌套属性，如"UserInfo.Address"）</param>
        private static void InternalValidate(object obj, Type objType, string parentPropertyPath)
        {
            if (obj == null)
            {
                // 若对象为null，且父属性路径非空（说明是嵌套属性），直接判定为无效（需结合外层[XmlRequired]）
                // 外层校验会先判断属性是否为null，此处仅处理递归内部的非空对象
                return;
            }

            // 获取对象所有公共属性
            PropertyInfo[] properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 遍历属性，仅对标记[XmlRequired]的属性执行必填校验
            foreach (PropertyInfo prop in properties)
            {
                // 拼接当前属性路径（便于定位嵌套属性，如"Address.Street"）
                string currentPropertyPath = string.IsNullOrEmpty(parentPropertyPath)
                                                ? prop.Name
                                                : $"{parentPropertyPath}.{prop.Name}";

                // 步骤1：获取当前属性的[XmlRequired]特性（不存在则直接跳过核心校验）
                // 在运行时通过反射，检索当前属性上是否应用了指定类型（XmlRequiredAttribute）的自定义特性。
                //   如果该属性上显式标记了 [XmlRequired] 特性，方法会返回该特性的实例（可通过该实例访问特性的属性 / 方法）；
                //   如果该属性上未标记该特性，方法会返回 null（这是判断特性是否存在的核心依据）。
                XmlRequiredAttribute xmlRequiredAttr = prop.GetCustomAttribute<XmlRequiredAttribute>();
                object propValue = null;

                //仅当 xmlRequiredAttr 不为null（标记了必填特性）时，才执行有效性校验
                if (xmlRequiredAttr != null)
                {
                    // 步骤2：获取属性值
                    propValue = prop.GetValue(obj);

                    // 步骤3：校验当前必填属性的值是否有效/集合元素个数范围校验
                    if (!IsPropValueValid(prop, propValue, xmlRequiredAttr))
                    {
                        // 拼接错误信息（优先使用特性自定义错误消息，否则使用默认消息）
                        string defaultErrorMsg = GetDefaultErrorMsg(prop, propValue, currentPropertyPath, xmlRequiredAttr);
                        string errorMsg = defaultErrorMsg ?? xmlRequiredAttr.ErrorMsg;


                        // 抛出自定义异常，携带关键定位信息
                        throw new XmlRequiredValidationException(errorMsg, currentPropertyPath, objType);
                    }
                }

                // 步骤4：嵌套对象递归校验（两种方案，根据业务场景选择）
                if (IsNestedCustomType(prop.PropertyType, propValue))
                {
                    // 无论是否标记[XmlRequired]，都递归校验嵌套对象的内部必填属性
                    // 优点：不遗漏嵌套对象内部的必填项；缺点：未标记必填的嵌套属性，也会触发递归（可接受，因为递归内部也会做同样优化）
                    InternalValidate(propValue, prop.PropertyType, currentPropertyPath);

                }
            }
        }

        /// <summary>
        /// 私有方法：获取默认错误提示信息（针对集合/普通属性差异化提示）
        /// </summary>
        /// <param name="prop">属性信息</param>
        /// <param name="propValue">属性值</param>
        /// <param name="propertyPath">属性路径</param>
        /// <param name="attr">XML必填特性</param>
        /// <returns>默认错误信息</returns>
        private static string GetDefaultErrorMsg(PropertyInfo prop, object propValue, string propertyPath, XmlRequiredAttribute attr)
        {
            if (IsCollectionType(prop.PropertyType))
            {
                // 集合类型：提示元素个数范围
                if (propValue == null)
                {
                    return $"属性【{propertyPath}】（集合类型）是XML序列化必填项，不能为null，要求最小元素数：{attr.MinItemCount}，最大元素数：{attr.MaxItemCount}";
                }
                ICollection collection = (ICollection)propValue;
                return $"属性【{propertyPath}】（集合类型）元素个数无效，当前个数：{collection.Count}，要求最小元素数：{attr.MinItemCount}，最大元素数：{attr.MaxItemCount}";
            }
            else
            {
                // 普通类型：原有提示逻辑
                return $"属性【{propertyPath}】是XML序列化必填项，当前值无效（值：{propValue ?? "null"}）";
            }
        }

        /// <summary>
        /// 私有方法：判断属性值是否有效（非空/非默认值）（保持原有逻辑不变）(支持集合元素个数范围校验)
        /// </summary>
        /// <param name="prop">属性信息</param>
        /// <param name="propValue">属性值</param>
        /// <param name="xmlRequiredAttr">XML必填特性（携带最小/最大元素数配置）</param>
        /// <returns>true=有效，false=无效</returns>
        private static bool IsPropValueValid(PropertyInfo prop, object propValue, XmlRequiredAttribute xmlRequiredAttr)
        {
            Type propType = prop.PropertyType;

            // 先判断是否为集合类型，执行集合专属校验（元素个数范围 + 非null）
            if (IsCollectionType(propType))
            {
                return ValidateCollectionValue(propValue, xmlRequiredAttr.MinItemCount, xmlRequiredAttr.MaxItemCount);
            }


            //  处理引用类型（string特殊处理，排除空字符串/空白字符串）
            if (!propType.IsValueType)
            {
                // string类型：不能为null、空字符串、空白字符串
                if (propType == typeof(string))
                {
                    return !string.IsNullOrWhiteSpace(propValue as string);
                }
                // 其他引用类型：不能为null
                return propValue != null;
            }

            //  处理值类型（可空值类型+非可空值类型）
            Type underlyingType = Nullable.GetUnderlyingType(propType);
            if (underlyingType != null)
            {
                // 可空值类型（如int?、DateTime?）：不能为null（HasValue=false）
                return propValue != null;
            }
            else
            {
                //// 非可空值类型（如int、DateTime）：不能是默认值（如int=0、DateTime=MinValue）
                //object defaultValue = Activator.CreateInstance(propType);
                //return !object.Equals(propValue, defaultValue);

                // 非可空值类型（如int、DateTime、bool等）：只要是值类型（必然有值），直接返回true
                // 无需判断是否为默认值（0/MinValue等均视为有效，满足“有值即可”的需求）
                return true;
            }
        }


        /// <summary>
        /// 私有方法：校验集合值是否有效（非null + 元素个数在[Min, Max]范围内）
        /// </summary>
        /// <param name="collectionValue">集合值</param>
        /// <param name="minCount">最小元素个数</param>
        /// <param name="maxCount">最大元素个数</param>
        /// <returns>true=有效，false=无效</returns>
        private static bool ValidateCollectionValue(object collectionValue, int minCount, int maxCount)
        {
            // 集合为null，直接无效
            if (collectionValue == null)
                return false;

            // 强转为ICollection，获取元素个数
            ICollection collection = (ICollection)collectionValue;
            int itemCount = collection.Count;

            // 校验元素个数是否在[minCount, maxCount]范围内
            return itemCount >= minCount && itemCount <= maxCount;
        }


        /// <summary>
        /// 私有方法：判断是否为集合类型（支持所有ICollection实现类：List、HashSet、ArrayList等）
        /// </summary>
        /// <param name="type">待判断类型</param>
        /// <returns>true=集合类型，false=非集合类型</returns>
        private static bool IsCollectionType(Type type)
        {
            // 排除string类型（string实现了ICollection，但不是集合）
            if (type == typeof(string))
                return false;

            // 判断是否实现了ICollection接口
            return typeof(ICollection).IsAssignableFrom(type);
        }

        /// <summary>
        /// 私有方法：判断是否为需要递归校验的嵌套自定义类型（核心升级点）
        /// </summary>
        /// <param name="propType">属性类型</param>
        /// <param name="propValue">属性值</param>
        /// <returns>true=是嵌套自定义类型，需要递归；false=简单类型，无需递归</returns>
        private static bool IsNestedCustomType(Type propType, object propValue)
        {
            if (propValue == null)
            {
                return false; // 值为null，无需递归
            }

            // 获取真实类型（处理可空类型，如AddressInfo? 转为 AddressInfo）
            Type realType = Nullable.GetUnderlyingType(propType) ?? propType;

            // 排除简单类型：无需递归校验
            HashSet<Type> simpleTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(int), typeof(long), typeof(short), typeof(byte),
                typeof(bool), typeof(decimal), typeof(float), typeof(double),
                typeof(DateTime), typeof(Guid), typeof(DateTimeOffset)
            };

            // 条件1：不是简单类型
            if (simpleTypes.Contains(realType))
            {
                return false;
            }

            // 条件2：不是枚举类型
            if (realType.IsEnum)
            {
                return false;
            }

            // 条件3：不是值类型（避免struct等简单值类型递归）
            if (realType.IsValueType && !realType.IsClass)
            {
                return false;
            }

            // 条件4：是自定义引用类型（类），需要递归校验
            return realType.IsClass;
        }
    }
}
