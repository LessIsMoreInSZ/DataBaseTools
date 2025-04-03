﻿namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 提示用户输入目标文件夹路径
            Console.WriteLine("请输入要扫描的目标文件夹路径:");
            string targetDirectory = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                Console.WriteLine("无效的目录路径。程序退出。");
                return;
            }

            // 获取所有 .cs 文件
            var csFiles = Directory.GetFiles(targetDirectory, "*.cs", SearchOption.AllDirectories);

            int modifiedCount = 0;
            int totalFiles = csFiles.Length;

            Console.WriteLine($"开始扫描 {totalFiles} 个 .cs 文件...");

            foreach (var filePath in csFiles)
            {
                try
                {
                    // 读取文件内容
                    var lines = File.ReadAllLines(filePath).ToList();

                    // 判断是否包含 "ZString"
                    bool containsZString = lines.Any(line => line.Contains("ZString", StringComparison.OrdinalIgnoreCase));

                    if (!containsZString)
                    {
                        // 找到所有 using Cysharp.Text; 的行索引
                        var usingDirectives = lines
                            .Select((line, index) => new { Line = line, Index = index })
                            .Where(item => item.Line.TrimStart().StartsWith("using Cysharp.Text;", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (usingDirectives.Any())
                        {
                            // 记录需要删除的行索引（从后往前删除，避免索引偏移）
                            var indicesToRemove = usingDirectives.Select(item => item.Index).Reverse().ToList();

                            foreach (var index in indicesToRemove)
                            {
                                lines.RemoveAt(index);
                            }

                            // 写回文件
                            File.WriteAllLines(filePath, lines);

                            modifiedCount++;
                            Console.WriteLine($"已修改: {filePath} （移除了 {usingDirectives.Count} 个 'using Cysharp.Text;' 指令）");
                        }
                        else
                        {
                            Console.WriteLine($"未修改: {filePath} （不包含 'using Cysharp.Text;'）");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"未修改: {filePath} （包含 'ZString'）");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理文件时出错 {filePath}: {ex.Message}");
                }
            }

            Console.WriteLine($"\n扫描完成。共修改了 {modifiedCount} 个文件。");
        }
    }
}
