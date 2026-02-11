# OFD Viewer

## 项目概述

OFD Viewer 是一个基于 .NET 8.0 开发的 ODF（Open Fixed-layout Document）文档处理库，用于读取、解析和写入 OFD 格式的文档。该项目旨在提供一个简单易用、功能完整的 OFD 文档处理解决方案，支持 .NET 8.0 及向下兼容 .NET Framework 4.0。

## 功能特性

- ✅ OFD 文档的读取和解析
- ✅ OFD 文档的写入和生成
- ✅ 支持 OFD 文档的基本结构解析
- ✅ 支持页面内容解析
- ✅ 支持字体处理
- ✅ 支持图形元素解析
- ✅ 支持注释处理
- ✅ 支持附件处理
- ✅ 支持数字签名处理
- ✅ 支持自定义标签处理

## 快速开始

### 环境要求

- .NET 8.0 SDK 或更高版本
- Visual Studio 2022 或其他兼容的 IDE

### 安装

将项目克隆到本地：

```bash
git clone https://github.com/yourusername/OfdViewer.git
```

然后在 Visual Studio 中打开解决方案文件 `OfdViewer.sln`，编译项目即可。

### 使用示例

#### 读取 OFD 文档

```csharp
using OFDViewer.OFD;

// 从文件路径打开 OFD 文档
using (var ofdDoc = OFDDocument.Open("sample.ofd"))
{
    // 获取文档基本信息
    Console.WriteLine($"文档页数: {ofdDoc.Pages.Count}");
    Console.WriteLine($"文档版本: {ofdDoc.Version}");
    
    // 处理文档内容
    // ...
}

// 从流中打开 OFD 文档
using (var stream = new FileStream("sample.ofd", FileMode.Open))
using (var ofdDoc = OFDDocument.Open(stream))
{
    // 处理文档内容
    // ...
}
```

#### 创建 OFD 文档

```csharp
using OFDViewer.OFD;
using OFDViewer.Models;

// 创建新的 OFD 文档
using (var ofdDoc = OFDDocument.Create())
{
    // 添加页面
    var page = ofdDoc.AddPage();
    
    // 添加内容到页面
    // ...
    
    // 保存文档
    ofdDoc.Save("new_sample.ofd");
}
```

## 核心模块说明

### OFDArchive
- 负责 OFD 文档的归档处理，包括 ZIP 压缩、解压、文件管理等
- 支持从文件和流两种方式打开 OFD 文档
- 提供 XML 文档的缓存机制，提高处理效率

### OFDDocument
- OFD 文档的核心类，提供文档的整体管理
- 包含文档的基本信息、页面集合、资源管理等

### OFDReader
- 负责 OFD 文档的读取和解析
- 将 OFD 文档的 XML 结构解析为内存中的对象模型

### OFDWriter
- 负责 OFD 文档的写入和生成
- 将内存中的对象模型转换为 OFD 文档的 XML 结构

### Models
- 包含 OFD 文档的所有数据模型
- 按照 OFD 标准的结构组织，便于解析和生成

## 开发指南

### 环境设置

1. 安装 .NET 8.0 SDK
2. 安装 Visual Studio 2022 或其他兼容 IDE
3. 克隆项目到本地
4. 打开解决方案文件 `OfdViewer.sln`

### 构建项目

在 Visual Studio 中，选择 "生成" -> "生成解决方案"，或使用命令行：

```bash
dotnet build
```

### 运行测试

在 Visual Studio 中，选择 "测试" -> "运行所有测试"，或使用命令行：

```bash
dotnet test
```

## 测试说明

测试项目包含了对 OFD 文档处理的各个方面的测试，包括：

- OFD 文档的读取和解析测试
- OFD 文档的写入和生成测试
- 各种模型类的序列化和反序列化测试
- 异常情况的处理测试

## 贡献指南

欢迎提交 Issue 和 Pull Request 来帮助改进项目。

### 提交 Pull Request 前的检查

1. 确保所有测试都通过
2. 确保代码符合项目的编码规范
3. 为新添加的功能编写相应的测试用例
4. 更新相关文档

## 许可证

本项目采用 MIT 许可证，详情请查看 LICENSE 文件。

## 联系信息

如有问题或建议，欢迎通过以下方式联系：

- GitHub Issues: https://github.com/yourusername/OfdViewer/issues

## 更新日志

### v1.0.0 (2026-01-18)
- 初始版本
- 支持 OFD 文档的基本读取和写入
- 支持页面内容解析
- 支持多种 OFD 元素处理

## 致谢

感谢所有为 OFD 标准和相关技术做出贡献的开发者和组织。
