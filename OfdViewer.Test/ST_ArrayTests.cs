using System.IO;
using System.Xml.Serialization;
using OFDViewer.Models.BaseType;
using Xunit;

namespace OFDViewer.Tests
{
    /// <summary>
    /// ST_Array XML序列化测试类
    /// </summary>
    public class ST_ArrayTests
    {
        /// <summary>
        /// 测试ST_Array的XML序列化和反序列化功能
        /// </summary>
        [Fact]
        public void ST_Array_XmlSerialization_Works()
        {
            // 测试用例1: 整数数组
            ST_Array originalArray1 = new ST_Array(1, 2, 3, 4, 5);
            VerifySerialization(originalArray1);
            
            // 测试用例2: 混合类型数组
            ST_Array originalArray2 = new ST_Array(1, 2.5, 3, "test", 4.7);
            VerifySerialization(originalArray2);
            
            // 测试用例3: 空数组
            ST_Array originalArray3 = new ST_Array();
            VerifySerialization(originalArray3);
            
            // 测试用例4: 单元素数组
            ST_Array originalArray4 = new ST_Array(42);
            VerifySerialization(originalArray4);
            
            // 测试用例5: 字符串数组
            ST_Array originalArray5 = new ST_Array("a", "b", "c");
            VerifySerialization(originalArray5);
        }
        
        /// <summary>
        /// 测试List<ST_Array>的XML序列化和反序列化功能
        /// </summary>
        [Fact]
        public void List_ST_Array_XmlSerialization_Works()
        {
            // 创建一个包含多个ST_Array的列表
            var originalList = new List<ST_Array>
            {
                new ST_Array(1, 2, 3),
                new ST_Array(4.5, 5.5, 6.5),
                new ST_Array("a", "b", "c")
            };
            
            // 序列化
            XmlSerializer serializer = new XmlSerializer(typeof(List<ST_Array>));
            using MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, originalList);
            
            // 重置流位置
            memoryStream.Position = 0;
            
            // 反序列化
            var deserializedList = (List<ST_Array>)serializer.Deserialize(memoryStream);
            
            // 验证结果
            Assert.NotNull(deserializedList);
            Assert.Equal(originalList.Count, deserializedList.Count);
            
            for (int i = 0; i < originalList.Count; i++)
            {
                Assert.Equal(originalList[i], deserializedList[i]);
            }
        }
        
        /// <summary>
        /// 验证单个ST_Array的序列化和反序列化
        /// </summary>
        /// <param name="originalArray">原始ST_Array对象</param>
        private void VerifySerialization(ST_Array originalArray)
        {
            // 序列化
            XmlSerializer serializer = new XmlSerializer(typeof(ST_Array));
            using MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, originalArray);
            
            // 查看生成的XML
            memoryStream.Position = 0;
            using StreamReader reader = new StreamReader(memoryStream);
            string xml = reader.ReadToEnd();
            Console.WriteLine($"Original: {originalArray}");
            Console.WriteLine($"XML: {xml}");
            
            // 重置流位置
            memoryStream.Position = 0;
            
            // 反序列化
            var deserializedArray = (ST_Array)serializer.Deserialize(memoryStream);
            Console.WriteLine($"Deserialized: {deserializedArray}");
            
            // 验证结果
            Assert.Equal(originalArray, deserializedArray);
            Assert.Equal(originalArray.Length, deserializedArray.Length);
            
            // 验证每个元素
            for (int i = 0; i < originalArray.Length; i++)
            {
                Assert.Equal(originalArray[i], deserializedArray[i]);
                Assert.Equal(originalArray[i].GetType(), deserializedArray[i].GetType());
            }
        }
    }
}