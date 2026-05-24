# DataBaseTools

工业现场常用的数据库维护工具集合，当前包含 3 个正式小工具：

1. `DataBaseTools`
   `.NET 6 + WPF` 的 SQL Server 维护工具，适合新环境机器。
2. `DataBaseTools.net48`
   `.NET Framework 4.8 + WPF` 的 SQL Server 维护工具，适合老环境机器。
3. `DataBaseTools.Sqlite`
   `.NET 6 + WinForms` 的 SQLite 维护工具。

`ConsoleApp1` 目前更像一个临时脚本型控制台工具，不在正式使用说明范围内。

## 这次优化内容

- 把 SQL Server 工具中的硬编码配置迁移到了配置文件里。
- 重做了两个 SQL Server 工具的主界面，让“读取表列表 / 读取概览 / 备份 / 删数 / 建索引”流程更清晰。
- 给删数操作补上了参数校验、表校验、备份文件校验和异常提示。
- 修复了 SQLite 工具里原本不可用的删除逻辑，改成真正的 SQLite SQL 语法。
- 补充了完整的中文使用手册。

## 配置入口

- `.NET 6 SQL Server`：`DataBaseTools/toolsettings.json`
- `.NET Framework 4.8 SQL Server`：`DataBaseTools.net48/App.config`
- `SQLite`：直接在界面里填写数据库文件、表名、列名和保留行数

## 构建说明

### DataBaseTools (.NET 6)

如果当前机器不能访问外网 NuGet，建议使用本地缓存和项目内 `packages` 目录做恢复：

```powershell
dotnet restore .\DataBaseTools\DataBaseTools.csproj `
  --source "$env:USERPROFILE\.nuget\packages" `
  --source ".\DataBaseTools\packages" `
  -p:NuGetAudit=false

dotnet build .\DataBaseTools\DataBaseTools.csproj --no-restore -p:NuGetAudit=false
```

### DataBaseTools.Sqlite (.NET 6)

```powershell
dotnet build .\DataBaseTools.Sqlite\DataBaseTools.Sqlite.csproj --no-restore
```

### DataBaseTools.net48 (.NET Framework 4.8)

需要传统的 Visual Studio / MSBuild WPF 构建环境。当前目录保留了旧版 `packages` 结构，可以在有完整 .NET Framework 桌面开发组件的机器上直接编译。

## 使用文档

详细步骤见：

- [使用说明手册.md](./使用说明手册.md)
