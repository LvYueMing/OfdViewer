using System.IO.Compression;
using System.Text;
using OFDViewer.OFD;
using Xunit;

namespace OFDViewer.Tests
{
    public class OFDArchiveTests : IDisposable
    {
        private readonly MemoryStream _zipStream;
        private readonly OFDArchive _archive;


        /*  目录结构
            ├── root_file.txt       // 根目录文件
            ├── another_root_file.xml
            ├── subdir/         // 根目录下的子目录
            │   ├── file1.txt    // 子目录文件
            │   ├── file3.txt
            │   ├── deep         // 子目录下的嵌套目录
            │   │   ├── nested/
            │   │   │   ├── file2.txt // 深层嵌套文件
            ├── second_dir/second_file.txt // 二级目录文件
        */
        public OFDArchiveTests()
        {
            // 在内存中创建 zip 包来模拟多层级文件结构
            _zipStream = new MemoryStream();
            using (var zip = new ZipArchive(_zipStream, ZipArchiveMode.Update, true))
            {
                // 直接位于根目录的文件
                var rootFile = zip.CreateEntry("root_file.txt");
                using (var writer = new StreamWriter(rootFile.Open()))
                {
                    writer.Write("Root file content");
                }

                // 根目录下的另一个文件
                var anotherRootFile = zip.CreateEntry("another_root_file.xml");
                using (var writer = new StreamWriter(anotherRootFile.Open()))
                {
                    writer.Write("<?xml version=\"1.0\"?><root><child>abc</child></root>");
                }

                // 直接位于根目录的子目录
                var subDirFile = zip.CreateEntry("subdir/file1.txt");
                using (var writer = new StreamWriter(subDirFile.Open()))
                {
                    writer.Write("Subdirectory file 1 content");
                }

                // 深层嵌套的文件
                var deepFile = zip.CreateEntry("subdir/deep/nested/file2.txt");
                using (var writer = new StreamWriter(deepFile.Open()))
                {
                    writer.Write("Deep nested file content");
                }

                // 同一目录下的另一个文件
                var anotherFile = zip.CreateEntry("subdir/file3.txt");
                using (var writer = new StreamWriter(anotherFile.Open()))
                {
                    writer.Write("Another file in subdir");
                }


                // 另一个子目录
                var secondSubDirFile = zip.CreateEntry("second_dir/second_file.txt");
                using (var writer = new StreamWriter(secondSubDirFile.Open()))
                {
                    writer.Write("Second subdirectory file content");
                }
            }
            _zipStream.Position = 0;
            _archive = OFDArchive.OpenFromStream(_zipStream, ZipArchiveMode.Read, leaveOpen: true);
        }

        [Fact]
        public void Open_FilePath_ShouldOpenArchive()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, _zipStream.ToArray());

            try
            {
                // Act
                using var archive = OFDArchive.OpenFromFile(tempFile);

                // Assert
                Assert.NotNull(archive);
            }
            finally
            {
                // 确保资源释放并清理文件
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Open_Stream_ShouldOpenArchive()
        {
            using var archive = OFDArchive.OpenFromStream(new MemoryStream(_zipStream.ToArray()));
            Assert.NotNull(archive);
        }

        [Fact]
        public void GetFileStream_ValidFile_ShouldReturnStream()
        {
            using var stream = _archive.OpenFileStream("root_file.txt");
            Assert.NotNull(stream);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Assert.Equal("Root file content", content);
        }

        [Fact]
        public void GetFileStream_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.OpenFileStream("notfound.txt"));
        }

        [Fact]
        public void ReadTextFile_ValidFile_ShouldReturnContent()
        {
            var content = _archive.ReadTextFile("root_file.txt");
            Assert.Equal("Root file content", content);
        }

        [Fact]
        public void ReadTextFile_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.ReadTextFile("notfound.txt"));
        }

        [Fact]
        public void ReadXmlFile_ValidFile_ShouldReturnXmlDocument()
        {
            var doc = _archive.ReadXmlFile("another_root_file.xml");
            Assert.NotNull(doc);
            Assert.Equal("root", doc.DocumentElement.Name);
            Assert.Equal("abc", doc.DocumentElement["child"].InnerText);
        }

        [Fact]
        public void ReadXmlFile_InvalidFile_ShouldThrow()
        {
            Assert.Throws<FileNotFoundException>(() => _archive.ReadXmlFile("notfound.xml"));
        }

        [Fact]
        public void ExtractToTempDirectory_ShouldExtractFiles()
        {
            var tempDir = _archive.ExtractToTempDirectory();
            try
            {
                var txtPath = Path.Combine(tempDir, "root_file.txt");
                var xmlPath = Path.Combine(tempDir, "another_root_file.xml");
                Assert.True(File.Exists(txtPath));
                Assert.True(File.Exists(xmlPath));
                Assert.Equal("Root file content", File.ReadAllText(txtPath));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }


        [Fact]
        public void ExtractAndReadOFD_LoadLocalFile()
        {
            var ofdPath = Path.Combine(@"..\..\..\..\OFD-File", "test.ofd");
            using var archive = OFDArchive.OpenFromFile(ofdPath);

            // 解压
            var tempDir = archive.ExtractToTempDirectory();
            Assert.True(Directory.Exists(tempDir));
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine($"已解压到{tempDir}");

            // 遍历所有文件项
            var entryCache = archive.GetType()
                .GetField("_entryCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(archive) as System.Collections.IDictionary;
            foreach (var entry in entryCache)
            {
                Console.WriteLine(((System.Collections.DictionaryEntry)entry).Key);
            }

            // 获取文档中的某个 XML 文件内容
            string xmlFile = "Doc_0/Document.xml"; // 替换为实际 OFD 文档中的 XML 路径
            var xml = archive.ReadXmlFile(xmlFile);
            Console.WriteLine(xml.OuterXml);

            // 清理
            Directory.Delete(tempDir, true);
        }


        #region GetDirectEntryNamesInDirectory

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("/")]
        public void GetDirectEntryNamesInDirectory_RootDirectory_ReturnsDirectFilesAndDirs(string root)
        {
            // Arrange
            var expectedEntries = new List<string> { "root_file.txt", "another_root_file.xml", "subdir", "second_dir" };

            // Act
            var result = _archive.GetDirectEntryNamesInDirectory(root); // ��Ŀ¼

            // Assert
            Assert.Equal(expectedEntries.OrderBy(x => x), result.OrderBy(x => x));
        }


        [Fact]
        public void GetDirectEntryNamesInDirectory_SubDirectory_ReturnsDirectChildrenOnly()
        {
            // Arrange
            var expectedEntries = new List<string> { "file1.txt", "file3.txt", "deep" }; // ע�⣺deep ����Ŀ¼

            // Act
            var result = _archive.GetDirectEntryNamesInDirectory("subdir");

            // Assert
            Assert.Equal(expectedEntries.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void GetDirectEntryNamesInDirectory_SecondSubDirectory_ReturnsDirectChild()
        {
            // Arrange
            var expectedEntries = new List<string> { "second_file.txt" };

            // Act
            var result = _archive.GetDirectEntryNamesInDirectory("second_dir");

            // Assert
            Assert.Equal(expectedEntries, result);
        }

        [Fact]
        public void GetDirectEntryNamesInDirectory_NestedDirectory_ReturnsDirectChild()
        {
            // Arrange
            var expectedEntries = new List<string> { "nested" }; // nested ����Ŀ¼

            // Act
            var result = _archive.GetDirectEntryNamesInDirectory("subdir/deep");

            // Assert
            Assert.Equal(expectedEntries, result);
        }

        // ·���淶������֤��ͬ·����ʽ��ͳһ����
        [Fact]
        public void GetDirectEntryNamesInDirectory_PathWithTrailingSlash_ReturnsSameResult()
        {
            // Arrange
            var expectedEntriesWithoutSlash = _archive.GetDirectEntryNamesInDirectory("subdir");
            var expectedEntriesWithSlash = _archive.GetDirectEntryNamesInDirectory("subdir/");

            // Act & Assert
            Assert.Equal(expectedEntriesWithoutSlash.OrderBy(x => x), expectedEntriesWithSlash.OrderBy(x => x));
        }


        //边界测试：传入不存在的目录路径
        [Fact]
        public void GetDirectEntryNamesInDirectory_NonExistentDirectory_ReturnsEmptyList()
        {
            // Act
            var result = _archive.GetDirectEntryNamesInDirectory("nonexistent");

            // Assert
            Assert.Empty(result);
        }

        // 异常测试：当 ZIP 归档为 null 时的情况
        [Fact]
        public void GetDirectEntryNamesInDirectory_ArchiveDisposed_ThrowsInvalidOperationException()
        {
            // Arrange
            var archive = OFDArchive.OpenFromStream(_zipStream, ZipArchiveMode.Read, leaveOpen: true);
            archive.Dispose(); // 显式释放归档

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => archive.GetDirectEntryNamesInDirectory(""));
            Assert.Equal("ZIP归档已释放，无法获取目录内容", exception.Message);
        }

        // 边界测试：测试路径格式的大小写
        [Fact]
        public void GetDirectEntryNamesInDirectory_NormalizePathFormat()
        {
            // 测试不同的路径格式应被标准化
            var result1 = _archive.GetDirectEntryNamesInDirectory("SUBDIR"); // 大写
            var result2 = _archive.GetDirectEntryNamesInDirectory("subdir"); // 小写

            // 注意：实际应为获取 NormalizePath 的实现，如果不进行小写转换
            // 但如果 NormalizePath 实现了小写转换，则预期结果可能会相同
            // 实际上不进行小写转换，所以结果应该不同
            Assert.NotEqual(result1.OrderBy(x => x), result2.OrderBy(x => x));
        }

        // 去重能力：确保 HashSet 去重机制正常工作  Duplicate-重复
        [Fact]
        public void GetDirectEntryNamesInDirectory_DuplicateEntries_RemovesDuplicates()
        {
            // 创建一个包含重复条目的测试压缩包
            var duplicateZipStream = new MemoryStream();
            using (var zip = new ZipArchive(duplicateZipStream, ZipArchiveMode.Update, true))
            {
                var entry1 = zip.CreateEntry("testdir/dupfile.txt");
                using (var writer = new StreamWriter(entry1.Open()))
                {
                    writer.Write("Duplicate file content");
                }

                // ����һ����������ͬ��·������������ʵ��ZIP�в�̫���ܷ�����
                var entry2 = zip.CreateEntry("testdir/dupfile.txt");
                using (var writer = new StreamWriter(entry2.Open()))
                {
                    writer.Write("Duplicate file content again");
                }
            }
            duplicateZipStream.Position = 0;

            using var duplicateArchive = OFDArchive.OpenFromStream(duplicateZipStream, ZipArchiveMode.Read, leaveOpen: true);

            // Act
            var result = duplicateArchive.GetDirectEntryNamesInDirectory("testdir");

            // Assert - Ӧ��û���ظ���
            var uniqueResults = new HashSet<string>(result);
            Assert.Equal(result.Count, uniqueResults.Count);
        }

        #endregion


        #region GetDirectFilePathsInDirectory

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("/")]
        public void GetDirectFilePathsInDirectory_RootDirectory_ReturnsDirectFilesAndDirs(string root)
        {
            // Arrange
            var expectedEntries = new List<string> { "root_file.txt", "another_root_file.xml" };

            // Act
            var result = _archive.GetDirectFilePathsInDirectory(root); // ��Ŀ¼

            // Assert
            Assert.Equal(expectedEntries.OrderBy(x => x), result.OrderBy(x => x));
        }


        [Fact]
        public void GetDirectFilePathsInDirectory_SubDirectory_ReturnsDirectChildrenOnly()
        {
            // Arrange
            var expectedEntries = new List<string> { "subdir/file1.txt", "subdir/file3.txt" }; // ע�⣺deep ����Ŀ¼

            // Act
            var result = _archive.GetDirectFilePathsInDirectory("subdir");

            // Assert
            Assert.Equal(expectedEntries.OrderBy(x => x), result.OrderBy(x => x));
        }

        [Fact]
        public void GetDirectFilePathsInDirectory_SecondSubDirectory_ReturnsDirectChild()
        {
            // Arrange
            var expectedEntries = new List<string> { "second_dir/second_file.txt" };

            // Act
            var result = _archive.GetDirectFilePathsInDirectory("second_dir");

            // Assert
            Assert.Equal(expectedEntries, result);
        }

        [Fact]
        public void GetDirectFilePathsInDirectory_NestedDirectory_ReturnsDirectChild()
        {
            // Act
            var result = _archive.GetDirectFilePathsInDirectory("subdir/deep");

            // Assert
            Assert.Empty(result);
        }

        // ·���淶������֤��ͬ·����ʽ��ͳһ����
        [Fact]
        public void GetDirectFilePathsInDirectory_PathWithTrailingSlash_ReturnsSameResult()
        {
            // Arrange
            var expectedEntriesWithoutSlash = _archive.GetDirectFilePathsInDirectory("subdir");
            var expectedEntriesWithSlash = _archive.GetDirectFilePathsInDirectory("subdir/");

            // Act & Assert
            Assert.Equal(expectedEntriesWithoutSlash.OrderBy(x => x), expectedEntriesWithSlash.OrderBy(x => x));
        }


        //边界测试：传入不存在的目录路径
        [Fact]
        public void GetDirectFilePathsInDirectory_NonExistentDirectory_ReturnsEmptyList()
        {
            // Act
            var result = _archive.GetDirectFilePathsInDirectory("nonexistent");

            // Assert
            Assert.Empty(result);
        }

        // 异常测试：当 ZIP 归档为 null 时的情况
        [Fact]
        public void GetDirectFilePathsInDirectory_ArchiveDisposed_ThrowsInvalidOperationException()
        {
            // Arrange
            var archive = OFDArchive.OpenFromStream(_zipStream, ZipArchiveMode.Read, leaveOpen: true);
            archive.Dispose(); // ��ʽ�ͷŵ���

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => archive.GetDirectFilePathsInDirectory(""));
            Assert.Equal("ZIP归档已释放，无法获取目录内容", exception.Message);
        }

        // 边界测试：测试路径格式的大小写
        [Fact]
        public void GetDirectFilePathsInDirectory_NormalizePathFormat()
        {
            // 测试不同的路径格式应被标准化
            var result1 = _archive.GetDirectFilePathsInDirectory("SUBDIR"); // 大写
            var result2 = _archive.GetDirectFilePathsInDirectory("subdir"); // 小写

            // 注意：实际应为获取 NormalizePath 的实现，如果不进行小写转换
            // 但如果 NormalizePath 实现了小写转换，则预期结果可能会相同
            // 实际上不进行小写转换，所以结果应该不同
            Assert.NotEqual(result1.OrderBy(x => x), result2.OrderBy(x => x));
        }

        // 去重能力：确保 HashSet 去重机制正常工作  Duplicate-重复
        [Fact]
        public void GetDirectFilePathsInDirectory_DuplicateEntries_RemovesDuplicates()
        {
            // 创建一个包含重复条目的测试压缩包
            var duplicateZipStream = new MemoryStream();
            using (var zip = new ZipArchive(duplicateZipStream, ZipArchiveMode.Update, true))
            {
                var entry1 = zip.CreateEntry("testdir/dupfile.txt");
                using (var writer = new StreamWriter(entry1.Open()))
                {
                    writer.Write("Duplicate file content");
                }

                // ����һ����������ͬ��·������������ʵ��ZIP�в�̫���ܷ�����
                var entry2 = zip.CreateEntry("testdir/dupfile.txt");
                using (var writer = new StreamWriter(entry2.Open()))
                {
                    writer.Write("Duplicate file content again");
                }
            }
            duplicateZipStream.Position = 0;

            using var duplicateArchive = OFDArchive.OpenFromStream(duplicateZipStream, ZipArchiveMode.Read, leaveOpen: true);

            // Act
            var result = duplicateArchive.GetDirectFilePathsInDirectory("testdir");

            // Assert - Ӧ��û���ظ���
            var uniqueResults = new HashSet<string>(result);
            Assert.Equal(result.Count, uniqueResults.Count);
        }

        #endregion


        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            _archive.Dispose();
        }


        public void Dispose()
        {
            _archive.Dispose();
            _zipStream.Dispose();
        }

    }
}
