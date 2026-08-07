# OFD Viewer

## 一、项目概述

OFD Viewer 是一个基于 .NET 的 OFD（Open Fixed-layout Document）文档处理与查看项目，用于读取、解析、创建、写入和渲染 OFD 文档。

项目以 GB/T 33190-2016《电子文件存储与交换格式 版式文档》和 GM/T 0031-2014《安全电子签章密码技术规范》为主要标准依据，标准文件和配套 XSD 位于 `Doc/`。

## 二、主要能力

- OFD ZIP 归档的文件和流式读写
- OFD.xml、Document.xml、页面、模板和资源解析
- 文本、图形、图像、注释、附件、自定义标签和签名数据模型
- OFD 文档创建、资源写入和归档输出
- 基于 SkiaSharp 的页面渲染
- 电子签章抽象、默认解析器和厂商解析器扩展机制
- Windows Forms 文档查看控件

电子签章模块当前可以解析部分结构、证书和印章图像，但国脉解析器的 SM2 深度验签尚未完成。该能力目前不能作为生产环境中的可信密码学验签结果。

## 三、项目结构

- `OfdViewer/`：`net8.0` 主类库，包含模型、归档、解析、写入和渲染能力。
- `OfdViewer.Eseal.Abstractions/`：`net8.0` 电子签章接口、模型、异常和工厂。
- `OfdViewer.Eseal.Implementations/`：`net8.0` 默认及厂商电子签章实现。
- `OfdViewer.Test/`：`net8.0` xUnit 自动化测试。
- `OfdViewer.WinForm/`：`net9.0-windows` Windows Forms 查看控件。
- `OfdViewer.WinForm.Test/`：`net9.0-windows` WinForms 手工宿主程序。
- `SkiaSharpExperiment/`：`net9.0-windows` 渲染实验程序。
- `Doc/`：OFD 标准、签章规范、XSD 和项目评估文档。

根目录的 `OfdViewer.csproj` 是不含业务代码的空壳项目；主类库项目文件是 `OfdViewer/OfdViewer.csproj`。

## 四、环境要求

- .NET 8 SDK：构建核心类库和运行自动化测试
- .NET 9 SDK：构建 WinForms 和实验项目
- Windows：运行 WinForms 项目，以及验证主类库中现有的 `System.Drawing` 相关路径
- Visual Studio 2022：可选；使用 WinForms 时需安装对应桌面开发工作负载

当前项目文件不支持 .NET Framework 4.0，不应将 `net8.0` 目标框架理解为对 .NET Framework 的向下兼容。

## 五、获取与构建

```powershell
git clone https://github.com/LvYueMing/OfdViewer.git
Set-Location OfdViewer
dotnet restore OfdViewer.sln
dotnet build OfdViewer.sln --configuration Debug --no-restore
```

仅构建核心类库：

```powershell
dotnet build OfdViewer/OfdViewer.csproj --configuration Debug
```

## 六、使用示例

### 读取 OFD 文档

```csharp
using OFDViewer.Parse;

using var reader = new OFDReader("sample.ofd");
var rootDocument = reader.ParseOFDDocument();

Console.WriteLine($"文档数量：{rootDocument.DocCount}");
Console.WriteLine($"默认文档页数：{rootDocument.DefaultOFDDocument.PageDocs.Count}");
```

从流读取并保持调用方流打开：

```csharp
using OFDViewer.Parse;

using var input = File.OpenRead("sample.ofd");
using var reader = new OFDReader(input, leaveOpen: true);
var rootDocument = reader.ParseOFDDocument();
```

### 创建并写入 OFD 文档

```csharp
using OFDViewer.Parse;

var rootDocument = new OFDRootDocument();
rootDocument.DefaultOFDDocument.NewPageDoc();

var outputPath = Path.Combine(Environment.CurrentDirectory, "new-sample.ofd");
using var writer = new OFDWriter(outputPath);
writer.WriteOFDRootDoc(rootDocument);
writer.Save();
```

### 渲染 OFD 页面

```csharp
using OFDViewer.Render;

using var renderer = new OfdRenderer("sample.ofd");
Console.WriteLine($"总页数：{renderer.PageCount}");
renderer.RenderPageToFile("page-1.png", pageIndex: 0);
```

## 七、运行测试

运行核心自动化测试：

```powershell
dotnet test OfdViewer.Test/OfdViewer.Tests.csproj --configuration Debug
```

运行指定测试类或测试方法：

```powershell
dotnet test OfdViewer.Test/OfdViewer.Tests.csproj --filter "FullyQualifiedName~OFDWriterTests"
```

测试必须使用仓库内夹具或临时目录，不得依赖开发机绝对路径和被忽略的 `OFD-File/` 目录。

## 八、开发约定

仓库级开发、测试、OFD 路径、渲染、电子签章和 Git 规则见 `AGENTS.md`。项目现状、风险和整改顺序见 `Doc/项目评估结果.md`。

提交 Pull Request 前应完成：

1. 运行与改动范围匹配的构建和测试。
2. 确认没有引入新的编译或分析器告警。
3. 为新增功能或缺陷修复补充自动化测试。
4. 更新受影响的公共 API 注释和文档。
5. 检查提交中不包含 `bin/`、`obj/`、`.vs/`、本机样例或其他忽略内容。

提交信息推荐使用 `<类型>(<范围>): <描述>`，例如：

```text
fix(OFD解析): 修复非标准目录下的页面定位
```

## 九、许可证与反馈

本项目采用 MIT 许可证，详情见根目录 `LICENSE` 文件。

MIT 许可证允许使用、复制、修改、发布、分发、再许可及商业使用本项目，但必须在软件副本或主要部分中保留原版权声明和许可证声明。软件按“原样”提供，不附带任何担保。

问题与建议请通过 GitHub Issues 提交：

https://github.com/LvYueMing/OfdViewer/issues
