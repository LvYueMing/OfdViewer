using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OFDViewer.BaseType;
using OFDViewer.Graph.ShapeItems;
using OFDViewer.Graph;
using Xunit;
using OFDViewer.Utils;

namespace OFDViewer.Tests
{
    public class RegionAreaTests
    {
        // 测试基础属性（StartPos）的序列化和反序列化
        [Fact]
        public void Serialize_Deserialize_WithStartPos_ReturnsEqualObject()
        {
            // 1. 准备测试数据
            var originalRegion = new RegionArea
            {
                Start = new ST_Pos(100, 200) // 假设 ST_Pos 有接收X/Y的构造函数
            };
            originalRegion.AddShapeItem(new Move());

            // 2. 执行序列化和反序列化
            XmlHelper.SerializeToFile(originalRegion,"RegionArea.xml");

            Console.WriteLine("序列化成功！");
        }


        //// 测试基础属性（StartPos）的序列化和反序列化
        //[Fact]
        //public void Serialize_Deserialize_WithStartPos_ReturnsEqualObject()
        //{
        //    // 1. 准备测试数据
        //    var originalRegion = new RegionArea
        //    {
        //        Start = new ST_Pos(100, 200) // 假设 ST_Pos 有接收X/Y的构造函数
        //    };

        //    // 2. 执行序列化和反序列化
        //    var serializedXml = XmlHelper.SerializeToFile(originalRegion, "RegionArea.xml");
        //    var deserializedRegion = DeserializeFromXml<RegionArea>(serializedXml);

        //    // 3. 验证结果
        //    Assert.NotNull(deserializedRegion);
        //    Assert.Equal(originalRegion.StartPos, deserializedRegion.StartPos);
        //    Assert.Equal(originalRegion.Start.X, deserializedRegion.Start.X); // 假设 ST_Pos 有X/Y属性
        //    Assert.Equal(originalRegion.Start.Y, deserializedRegion.Start.Y);
        //}

        //// 测试单一图形元素（如Move）的序列化和反序列化
        //[Fact]
        //public void Serialize_Deserialize_WithSingleMoveItem_ReturnsEqualObject()
        //{
        //    // 1. 准备测试数据
        //    var originalRegion = new RegionArea
        //    {
        //        Start = new ST_Pos(50, 50)
        //    };
        //    var moveItem = new Move { X = 10, Y = 20 }; // 假设 Move 有X/Y属性
        //    originalRegion.AddShapeItem(moveItem);

        //    // 2. 执行序列化和反序列化
        //    var serializedXml = SerializeToXml(originalRegion);
        //    var deserializedRegion = DeserializeFromXml<RegionArea>(serializedXml);

        //    // 3. 验证结果
        //    Assert.NotNull(deserializedRegion);
        //    Assert.Single(deserializedRegion.ShapeItems);
        //    Assert.Equal(1, deserializedRegion.ShapeItemNames.Length);
        //    Assert.Equal(ShapeItemEnum.Move, deserializedRegion.ShapeItemNames[0]);

        //    var deserializedMove = deserializedRegion.ShapeItems[0] as Move;
        //    Assert.NotNull(deserializedMove);
        //    Assert.Equal(moveItem.X, deserializedMove.X);
        //    Assert.Equal(moveItem.Y, deserializedMove.Y);
        //}

        //// 测试多种图形元素混合的序列化和反序列化
        //[Fact]
        //public void Serialize_Deserialize_WithMultipleShapeItems_ReturnsEqualObject()
        //{
        //    // 1. 准备测试数据
        //    var originalRegion = new RegionArea
        //    {
        //        Start = new ST_Pos(0, 0)
        //    };
        //    originalRegion.AddShapeItem(new Move { X = 10, Y = 10 });
        //    originalRegion.AddShapeItem(new Line { X1 = 10, Y1 = 10, X2 = 20, Y2 = 20 }); // 假设 Line 有X1/Y1/X2/Y2属性
        //    originalRegion.AddShapeItem(new Close()); // 无属性的Close元素

        //    // 2. 执行序列化和反序列化
        //    var serializedXml = SerializeToXml(originalRegion);
        //    var deserializedRegion = DeserializeFromXml<RegionArea>(serializedXml);

        //    // 3. 验证结果
        //    Assert.NotNull(deserializedRegion);
        //    Assert.Equal(3, deserializedRegion.ShapeItems.Count);
        //    Assert.Equal(3, deserializedRegion.ShapeItemNames.Length);

        //    // 验证枚举顺序和类型匹配
        //    Assert.Equal(ShapeItemEnum.Move, deserializedRegion.ShapeItemNames[0]);
        //    Assert.Equal(ShapeItemEnum.Line, deserializedRegion.ShapeItemNames[1]);
        //    Assert.Equal(ShapeItemEnum.Close, deserializedRegion.ShapeItemNames[2]);

        //    // 验证每个元素的类型和属性
        //    Assert.IsType<Move>(deserializedRegion.ShapeItems[0]);
        //    Assert.IsType<Line>(deserializedRegion.ShapeItems[1]);
        //    Assert.IsType<Close>(deserializedRegion.ShapeItems[2]);

        //    var deserializedLine = deserializedRegion.ShapeItems[1] as Line;
        //    Assert.Equal(20, deserializedLine.X2);
        //    Assert.Equal(20, deserializedLine.Y2);
        //}

        //// 测试不支持的图形类型添加（验证异常）
        //[Fact]
        //public void AddShapeItem_WithUnsupportedType_ThrowsArgumentException()
        //{
        //    // 1. 准备测试数据
        //    var region = new RegionArea();
        //    var unsupportedItem = new object(); // 非目标类型的对象

        //    // 2. 验证抛出异常
        //    var exception = Assert.Throws<ArgumentException>(() => region.AddShapeItem(unsupportedItem));
        //    Assert.Contains("不支持的图形类型：Object", exception.Message);
        //}
    }
}
