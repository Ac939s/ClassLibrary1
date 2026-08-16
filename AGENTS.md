# KillSounds 项目指令

RimWorld 1.6 击杀音效模组（C# / .NET Framework 4.7.2 / Harmony）。

## 仓库结构

```
Source/KillSounds/            # git 仓库根（= opencode 项目根）
├── code/                     # C# 源码（KillSounds.slnx / KillSounds.csproj）
├── doc/                      # 项目文档（README.md 总览、design.md 设计、CODE_STANDARDS.md 规范）
└── .opencode/                # agent 团队配置（agents/）、记忆（memory/）、团队文档（docs/）
```

完整模组发布内容（`About/`、`Languages/`、`Sounds/`、`1.6/`）位于仓库上级目录 `Mods/KillSounds/`。

## 构建

```bash
dotnet build code/KillSounds.csproj -c Debug
```

要求 0 编译错误（0 警告优先）。构建成功自动复制 `KillSounds.dll` 到模组 `1.6/Assemblies/`。

## 工作流

本项目使用 6 人 Agent 团队，由 team-lead 协调，每个阶段需用户审核：

```
@planner → 用户审核策划案
  → @architect → 用户审核方案
    → @programmer → 用户审核代码（说"同意"才能继续）
      → @docs-updater → team-lead 更新工作日志 → @git-admin
```

- 团队配置：`.opencode/docs/AGENTS_CONFIG.md`
- 各角色定义：`.opencode/agents/*.md`
- 永久记忆：`.opencode/memory/MEMORY.md`（每次会话必读）
- 工作日志：`.opencode/memory/logs/YYYY-MM-DD.md`

## 常用约定

- 语言：中文
- RimWorld API（Verse/RimWorld 类型、Def、Harmony 补丁）用 RimSearcher MCP 工具查询，不要凭记忆猜
- 编码规范见 `doc/CODE_STANDARDS.md`
