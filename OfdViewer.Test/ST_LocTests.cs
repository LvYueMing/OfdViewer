using OFDViewer.Models.BaseType;
using Xunit;

namespace OFDViewer.Tests
{
    public class ST_LocTests
    {
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

