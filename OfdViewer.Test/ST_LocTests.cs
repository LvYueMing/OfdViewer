using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using OFDViewer.Models.BaseType;
using Xunit;

namespace OFDViewer.Tests
{
    public class ST_LocTests
    {
        [Fact]
        public void Equals_Works()
        {
            var loc1 = new ST_Loc("a/b/c");
            var loc2 = new ST_Loc("a/b/c");
            var loc3 = new ST_Loc("a/b/d");
            Assert.True(loc1.Equals(loc2));
            Assert.False(loc1.Equals(loc3));
            Assert.True(loc1.Equals((object)loc2));
            Assert.False(loc1.Equals((object)loc3));
            Assert.False(loc1.Equals(null));
        }

        [Fact]
        public void GetHashCode_Works()
        {
            var loc1 = new ST_Loc("a/b/c");
            var loc2 = new ST_Loc("a/b/c");
            var loc3 = new ST_Loc("a/b/d");
            Assert.Equal(loc1.GetHashCode(), loc2.GetHashCode());
            Assert.NotEqual(loc1.GetHashCode(), loc3.GetHashCode());
        }

        [Fact]
        public void OperatorEquals_Works()
        {
            var loc1 = new ST_Loc("a/b/c");
            var loc2 = new ST_Loc("a/b/c");
            var loc3 = new ST_Loc("a/b/d");
            Assert.True(loc1 == loc2);
            Assert.False(loc1 == loc3);
            Assert.True(loc1 != loc3);
            Assert.False(loc1 != loc2);
        }

        // 测试ST_Loc对象的XML序列化和反序列化
        [Fact]
        public void ST_Loc_XmlSerialization_Works()
        {
            // 准备测试数据
            var originalLoc = new ST_Loc("a/b/c/d.xml");

            // 序列化
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(ST_Loc));
                serializer.Serialize(memoryStream, originalLoc);

                // 反序列化
                memoryStream.Position = 0;
                var deserializedLoc = (ST_Loc)serializer.Deserialize(memoryStream);

                // 验证
                Assert.Equal(originalLoc.Path, deserializedLoc.Path);
                Assert.Equal(originalLoc.ToString(), deserializedLoc.ToString());
                Assert.True(originalLoc.Equals(deserializedLoc));
                Assert.True(originalLoc == deserializedLoc);
            }
        }

        // 测试List<ST_Loc>集合的XML序列化和反序列化
        [Fact]
        public void List_ST_Loc_XmlSerialization_Works()
        {
            // 准备测试数据
            var originalList = new List<ST_Loc>
            {
                new ST_Loc("a/b/c.xml"),
                new ST_Loc("d/e/f.xml"),
                new ST_Loc("g/h/i.xml"),
                new ST_Loc("j/k/l.xml")
            };

            // 序列化
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(List<ST_Loc>));
                serializer.Serialize(memoryStream, originalList);

                // 反序列化
                memoryStream.Position = 0;
                var deserializedList = (List<ST_Loc>)serializer.Deserialize(memoryStream);

                // 验证
                Assert.Equal(originalList.Count, deserializedList.Count);
                for (int i = 0; i < originalList.Count; i++)
                {
                    Assert.Equal(originalList[i].Path, deserializedList[i].Path);
                    Assert.Equal(originalList[i].ToString(), deserializedList[i].ToString());
                    Assert.True(originalList[i].Equals(deserializedList[i]));
                    Assert.True(originalList[i] == deserializedList[i]);
                }
            }
        }

        // Theory
        [Theory]
        [InlineData(null, ".")]
        [InlineData("", ".")]
        [InlineData(".", ".")]
        [InlineData("/a/b/c", "a/b/c")]
        [InlineData("a/b/c", "a/b/c")]
        [InlineData("./a/b", "a/b")]
        [InlineData("../a/b", "../a/b")]
        [InlineData("/a/./b/../c", "a/c")]
        [InlineData("a/./b/../c", "a/c")]
        [InlineData("./a/./b/../c", "a/c")]
        [InlineData("/../a", "../a")]
        [InlineData("../../a", "../../a")]
        [InlineData("./../../a", "../../a")]
        public void Constructor_And_Properties_Works(string input, string expectedPath)
        {
            var loc = new ST_Loc(input);
            Assert.Equal(expectedPath, loc.Path);
        }

        [Fact]
        public void ToString_ReturnsPath()
        {
            var loc = new ST_Loc("abc/def");
            Assert.Equal("abc/def", loc.ToString());
        }

        // 测试Resolve方法的路径解析功能
        // 特别验证用户提到的场景：当前路径base/path/sub，解析../../a得到base/a
        [Theory]
        [InlineData(".", "base/path", "base/path")]
        [InlineData("sub/file.xml", "base/path", "base/path/sub/file.xml")]
        [InlineData("../a", "base/path", "base/a")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        [InlineData("./sub/file", "base/path", "base/path/sub/file")]
        [InlineData("sub/../file", "base/path", "base/path/file")]
        [InlineData("../sub/../file", "base/path", "base/file")]
        [InlineData("a/b/c", ".", "a/b/c")]
        [InlineData("a", ".", "a")]
        [InlineData("sub/file", "base", "base/sub/file")]
        [InlineData("Doc_0/Signs/Sign_0/Signature.xml", "Doc_0/Signs", "Doc_0/Signs/Sign_0/Signature.xml")]
        [InlineData("./Res/image_10.bmp", "Doc_0/Res", "Doc_0/Res/image_10.bmp")]
        [InlineData("Res/image_10.bmp", "Doc_0/Res", "Doc_0/Res/image_10.bmp")]
        [InlineData("Res/sub/image_10.bmp", "Doc_0/Res/sub", "Doc_0/Res/sub/image_10.bmp")]
        [InlineData("../Doc_0/Res/image_18.png", "Doc_0/Res", "Doc_0/Res/image_18.png")]
        public void GetAbsolutePath_RelativePath_Against_BaseLoc(string relativePath, string baseLoc, string expectedPath)
        {
            var loc = new ST_Loc(relativePath);
            var baseLocation = new ST_Loc(baseLoc);
            var resolved = loc.GetAbsolutePath(baseLocation);
            Assert.Equal(expectedPath, resolved.Path);
        }

        // 测试异常情况：相对路径超出基准路径
        [Theory]
        // 基准路径为空或当前目录，相对路径包含..
        [InlineData("..", ".")]
        [InlineData("../a", ".")]
        [InlineData("../../a", ".")]
        // 相对路径中的..数量超过基准路径层级
        [InlineData("../../a", "base")]
        [InlineData("../../../a", "base/path")]
        [InlineData("../../../../a", "base/path/sub")]
        public void GetAbsolutePath_ThrowsException_When_RelativePathExceedsBasePath(string relativePath, string baseLoc)
        {
            var loc = new ST_Loc(relativePath);
            var baseLocation = new ST_Loc(baseLoc);
            Assert.Throws<ArgumentException>(() => loc.GetAbsolutePath(baseLocation));
        }

        // 测试静态Resolve方法（字符串参数版本）
        [Theory]
        [InlineData("sub/file.xml", "base/path", "base/path/sub/file.xml")]
        [InlineData("../a", "base/path", "base/a")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        public void GetAbsolutePath_StaticMethod_StringParameters(string relativePath, string baseLoc, string expectedPath)
        {
            var resolved = ST_Loc.GetAbsolutePath(relativePath, baseLoc);
            Assert.Equal(expectedPath, resolved.Path);
        }

        // 测试静态Resolve方法（ST_Loc参数版本）
        [Theory]
        [InlineData("sub/file.xml", "base/path", "base/path/sub/file.xml")]
        [InlineData("../a", "base/path", "base/a")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        public void GetAbsolutePath_StaticMethod_STLocParameters(string relativePath, string baseLoc, string expectedPath)
        {
            var loc = new ST_Loc(relativePath);
            var baseLocation = new ST_Loc(baseLoc);
            var resolved = ST_Loc.GetAbsolutePath(loc, baseLocation);
            Assert.Equal(expectedPath, resolved.Path);
        }


        // 测试GetRelativePath方法
        [Theory]
        [InlineData("a/b/c", "a/b/c", ".")] // 相同路径
        [InlineData("a/b/c", "a/b", "c")] // 目标是基准的子路径
        [InlineData("a/b", "a/b/c", "..")] // 目标是基准的父路径
        [InlineData("a/c", "a/b", "../c")] // 目标是基准的同级目录
        [InlineData("a/b/c/d", "a", "b/c/d")] // 目标是基准的多级子路径
        [InlineData("a", "a/b/c/d", "../../..")] // 目标是基准的多级父路径
        [InlineData("a/b/c/d", "a/b/x/y", "../../c/d")] // 目标与基准有共同父目录
        [InlineData("Doc_0/PublicRes.xml", "Doc_0/Document.xml", "PublicRes.xml")] // 用户提到的具体场景
        [InlineData(".", "a/b/c", ".")] // 目标是当前目录，表示与基准路径在同一路径
        [InlineData("a/b/c", ".", "a/b/c")] // 基准是当前目录
        [InlineData("a/b/c", "", "a/b/c")] // 基准是空字符串
        public void GetRelativePath_Works(string targetPath, string basePath, string expectedPath)
        {
            var targetLoc = new ST_Loc(targetPath);
            var baseLoc = new ST_Loc(basePath);
            var relativeLoc = targetLoc.GetRelativePath(baseLoc);
            Assert.Equal(expectedPath, relativeLoc.Path);
        }

        // 测试静态GetRelativePath方法（ST_Loc参数版本）
        [Theory]
        [InlineData("a/b/c", "a/b", "c")]
        [InlineData("Doc_0/PublicRes.xml", "Doc_0/Document.xml", "PublicRes.xml")] // 用户提到的具体场景
        public void GetRelativePath_StaticMethod_STLocParameters(string targetPath, string basePath, string expectedPath)
        {
            var targetLoc = new ST_Loc(targetPath);
            var baseLoc = new ST_Loc(basePath);
            var relativeLoc = ST_Loc.GetRelativePath(targetLoc, baseLoc);
            Assert.Equal(expectedPath, relativeLoc.Path);
        }

        // 测试静态GetRelativePath方法（字符串参数版本）
        [Theory]
        [InlineData("a/b/c", "a/b", "c")]
        [InlineData("Doc_0/PublicRes.xml", "Doc_0/Document.xml", "PublicRes.xml")] // 用户提到的具体场景
        public void GetRelativePath_StaticMethod_StringParameters(string targetPath, string basePath, string expectedPath)
        {
            var relativeLoc = ST_Loc.GetRelativePath(targetPath, basePath);
            Assert.Equal(expectedPath, relativeLoc);
        }

        // 测试异常情况：目标路径和基准路径没有共同父路径
        [Theory]
        [InlineData("x/y/z", "a/b/c")] // 完全不同的路径
        [InlineData("dir1/file.txt", "dir2/file.txt")] // 不同目录下的文件
        [InlineData("a/b", "c/d")] // 不同的一级目录
        public void GetRelativePath_ThrowsException_When_NoCommonParentPath(string targetPath, string basePath)
        {
            var targetLoc = new ST_Loc(targetPath);
            var baseLoc = new ST_Loc(basePath);
            Assert.Throws<ArgumentException>(() => targetLoc.GetRelativePath(baseLoc));
        }
    }
}

