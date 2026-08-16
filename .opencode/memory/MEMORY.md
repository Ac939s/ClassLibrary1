# MEMORY.md — 永久记忆

> 本文件由 Team Lead 维护，每次会话开始必须读取。

## 项目概况

- 名称：KillSounds（击杀音效）
- 类型：RimWorld 1.6 模组（C# / .NET Framework 4.7.2 / Harmony）
- 包 ID：`xhpp.KillSounds`；作者：xhpp
- 功能：敌对单位被玩家/友军击杀时播放音效；支持连杀音效、尖叫开关、连杀浮动文字（视觉反馈）

## 仓库结构

```
Source/KillSounds/            # git 仓库根（= opencode 项目根）
├── code/                     # C# 工程（KillSounds.slnx / KillSounds.csproj）
│   ├── HarmonyInit.cs        # Harmony 初始化 + Def 校验
│   ├── Patch_Pawn_Kill.cs    # 补丁 Pawn.Kill，主逻辑入口
│   ├── StreakTracker.cs      # 连杀计数
│   ├── SoundPicker.cs        # 选音效
│   ├── DefOf.cs              # 音效 DefOf
│   ├── Settings.cs           # ModSettings + 设置界面
│   ├── Mod.cs                # Mod 子类
│   └── VisualFeedback.cs     # 连杀浮动文字
├── doc/                      # 项目文档
│   ├── README.md             # 总览
│   ├── design.md             # 扩展设计
│   └── CODE_STANDARDS.md     # 编码规范
└── .opencode/                # agent 团队配置（agents/）、记忆（memory/）、团队文档（docs/）
```

注意：完整模组发布内容（`About/`、`Languages/`、`Sounds/`、`1.6/`）位于仓库上级目录 `Mods/KillSounds/`。构建后 DLL 自动复制到 `1.6/Assemblies/`。

## 技术栈与关键 API

- 依赖：Harmony（工坊 2009463077）
- 关键类型：`Pawn.Kill(DamageInfo?, Hediff)`、`DamageInfo`（Def/Instigator/Weapon/HitPart）、`SoundDef`、`MoteMaker.ThrowText`、`DefOf`、`ModSettings`
- RimWorld API 一律用 RimSearcher MCP 工具查询（locate / inspect / read_code / trace / search_regex），不凭记忆猜

## 核心架构（当前管线）

```
Pawn.Kill → Patch_Pawn_Kill.Postfix
  → Settings 检查 → 取击杀者（dinfo.Instigator）
  → ShouldTrigger（派系判断）
  → StreakTracker.RecordKill（连杀计数）
  → SoundPicker.ChooseSound（普通池 + 2~6 连杀音效）
  → 播放音效
  → VisualFeedback.Trigger（连杀浮动文字，击杀者头顶）
```

## 扩展方向（已设计于 doc/design.md）

1. 触发条件多样化（武器/目标类型/伤害类型等）✅ 已批准待实现
2. 事件类型（爆头/处刑/爆炸/火焰/误杀）—— 依赖触发条件系统的 KillContext
3. 视觉反馈 ✅（基础版完成）
4. 设置界面 Tab 化

实现顺序建议：视觉反馈 → 触发条件（引入 KillContext 管线）→ 事件类型 → 设置界面

## 团队工作流

6 人团队：planner → architect → programmer → docs-updater → git-admin，由 team-lead 协调，每个阶段用户审核。详见 `.opencode/docs/AGENTS_CONFIG.md` 与 `.opencode/agents/`。

## 编码规范

见 `doc/CODE_STANDARDS.md`。核心：C# PascalCase 方法/类、camelCase 私有字段；构建 `dotnet build code/KillSounds.csproj -c Debug`；不主动加注释。

## 当前进度

### 已完成（2026-08-16）
- 目录整理：源码入 `code/`、文档入 `doc/`
- 源码文件去前缀重命名（SoundPicker/DefOf/Settings/Mod/StreakTracker）
- 初始化 doc/README.md、doc/design.md
- 视觉反馈基础版（连杀浮动文字）
- 搭建 6 人 agent 团队 + 记忆系统

### 待办（已批准未实现）
- 移除震屏（Settings/VisualFeedback/翻译 key）
- 浮动文字从死亡位置改为击杀者头顶（`killer.DrawPos + Vector3.up * 1.5f`，保留两级开关）

### 待办（设计已定，待走流程）
- 触发条件系统（KillContext 管线重构）
- 事件类型、设置界面 Tab 化
