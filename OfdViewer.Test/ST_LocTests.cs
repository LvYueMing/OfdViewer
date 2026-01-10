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
        [InlineData("../../a", "base/path", "a")]
        [InlineData("../../a/b", "base/path", "a/b")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        [InlineData("../../../a", "base/path/sub", "a")]
        [InlineData("../../../../a", "base/path/sub", "a")]
        [InlineData("./sub/file", "base/path", "base/path/sub/file")]
        [InlineData("sub/../file", "base/path", "base/path/file")]
        [InlineData("../sub/../file", "base/path", "base/file")]
        [InlineData("../../sub/../../file", "base/path", "file")]
        [InlineData("a/b/c", ".", "a/b/c")]
        [InlineData("../a", ".", "a")]
        [InlineData("../../a", ".", "a")]
        [InlineData("../../../a", ".", "a")]
        public void Resolve_RelativePath_Against_BaseLoc(string relativePath, string baseLoc, string expectedPath)
        {
            var loc = new ST_Loc(relativePath);
            var baseLocation = new ST_Loc(baseLoc);
            var resolved = loc.Resolve(baseLocation);
            Assert.Equal(expectedPath, resolved.Path);
        }

        // 测试静态Resolve方法（字符串参数版本）
        [Theory]
        [InlineData("sub/file.xml", "base/path", "base/path/sub/file.xml")]
        [InlineData("../a", "base/path", "base/a")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        public void Resolve_StaticMethod_StringParameters(string relativePath, string baseLoc, string expectedPath)
        {
            var resolved = ST_Loc.Resolve(relativePath, baseLoc);
            Assert.Equal(expectedPath, resolved.Path);
        }

        // 测试静态Resolve方法（ST_Loc参数版本）
        [Theory]
        [InlineData("sub/file.xml", "base/path", "base/path/sub/file.xml")]
        [InlineData("../a", "base/path", "base/a")]
        [InlineData("../../a", "base/path/sub", "base/a")] // 用户提到的具体场景
        public void Resolve_StaticMethod_STLocParameters(string relativePath, string baseLoc, string expectedPath)
        {
            var loc = new ST_Loc(relativePath);
            var baseLocation = new ST_Loc(baseLoc);
            var resolved = ST_Loc.Resolve(loc, baseLocation);
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
    }
}

