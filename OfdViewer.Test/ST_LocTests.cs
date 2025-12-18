using OFDViewer.BaseType;
using System;
using Xunit;

namespace OFDViewer.Tests
{
    public class ST_LocTests
    {
        // Theory：表示这是参数化测试
        [Theory]
        // InlineData：为测试方法提供一组参数（按顺序匹配方法参数）
        [InlineData(null, ".", false)]
        [InlineData("", ".", false)]
        [InlineData(".", ".", false)]
        [InlineData("/a/b/c", "/a/b/c", true)]
        [InlineData("a/b/c", "./a/b/c", false)]
        [InlineData("./a/b", "./a/b", false)]
        [InlineData("../a/b", "../a/b", false)]
        [InlineData("/a/./b/../c", "/a/c", true)]
        [InlineData("a/./b/../c", "./a/c", false)]
        [InlineData("/../a", "/a", true)]
        [InlineData("../../a", "../../a", false)]
        public void Constructor_And_Properties_Works(string input, string expectedPath, bool expectedIsAbsolute)
        {
            var loc = new ST_Loc(input);
            Assert.Equal(expectedPath, loc.Path);
            Assert.Equal(expectedIsAbsolute, loc.IsAbsolute);
            Assert.Equal(!expectedIsAbsolute, loc.IsRelative);
        }

        [Fact]
        public void ToString_ReturnsPath()
        {
            var loc = new ST_Loc("abc/def");
            Assert.Equal("./abc/def", loc.ToString());
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
