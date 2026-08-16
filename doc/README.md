# KillSounds

击杀音效模组 —— 在环世界（RimWorld）1.6 中，当敌对单位被玩家或友军击杀时，在死亡位置播放击杀音效；支持连杀递增音效、可选尖叫声等。

## 目录结构

```
KillSounds/                        # 模组根目录（发布内容）
├── About/                         # 模组元数据（About.xml、预览图、工坊 ID）
├── Languages/                     # 翻译文件（英文 / 简体中文）
├── Sounds/                        # 音效源文件（.ogg）
├── 1.6/
│   ├── Assemblies/                # 编译产物（KillSounds.dll，构建时自动复制）
│   └── Defs/SoundDefs.xml         # 音效 Def 定义
└── Source/KillSounds/             # 源码仓库
    ├── code/                      # C# 工程（KillSounds.slnx / KillSounds.csproj）
    └── doc/                       # 项目文档
```

## 功能与工作原理

通过 Harmony 补丁 `Pawn.Kill`，在每次角色死亡时：

1. 从 `DamageInfo` 获取击杀者（Instigator）。
2. 根据设置判断是否触发（默认仅玩家阵营击杀敌对单位触发，可开启友军击杀）。
3. 记录击杀连击（`StreakTracker`），同一时间窗口（默认 10 秒）内的连续击杀数递增。
4. 由 `SoundPicker` 根据设置与连击数选择音效 Def。
5. 在死亡位置播放音效。

### 源码模块（code/）

| 文件 | 职责 |
| --- | --- |
| `HarmonyInit.cs` | `[StaticConstructorOnStartup]` 初始化 Harmony、`PatchAll()`，并校验必需的音效 Def 是否存在 |
| `Patch_Pawn_Kill.cs` | 补丁 `Pawn.Kill` 的逻辑入口，负责触发判定与播放 |
| `StreakTracker.cs` | 连杀统计：按击杀者记录连击数与时间窗口，定期清理过期数据 |
| `SoundPicker.cs` | 根据设置与连击数选择 `SoundDef`（连杀音效优先，其次音效池） |
| `DefOf.cs` | 音效 Def 的静态引用（`[RimWorld.DefOf]`） |
| `Settings.cs` | `ModSettings`：设置项存储与设置界面绘制 |
| `Mod.cs` | `Mod` 子类，承载设置界面 |

## 音效 Def（1.6/Defs/SoundDefs.xml）

| defName | 用途 |
| --- | --- |
| `KillSoundPool_All` | 全部击杀音效池（含惨叫），默认音效池 |
| `KillSoundPool_NoScream` | 无惨叫音效池（关闭尖叫选项时使用） |
| `KillSoundPool_Battlefield` | 战地音效池（仅战地模式时使用） |
| `KillSound2_CF` ~ `KillSound6_CF` | 2~6 连杀专属音效（达到对应连击数时播放） |

## 设置项（游戏内 Mod 设置）

| 设置 | 说明 |
| --- | --- |
| 启用模组 | 总开关 |
| 友军击杀触发 | 允许友军（非敌对玩家阵营）击杀也触发音效 |
| 播放尖叫 | 是否包含尖叫类音效 |
| 连杀音效 | 开启后按连击数播放递增音效 |
| 连杀时间窗口 | 判定连击的间隔秒数（1~30s，默认 10s） |
| 仅战地音效 | 只使用战地音效池 |
| 调试日志 | 在日志中输出音效选择与播放信息 |

## 构建

环境：.NET Framework 4.7.2，依赖 `Assembly-CSharp.dll`、`UnityEngine*.dll`（游戏根目录）与 `0Harmony.dll`（工坊 Harmony 模组）。

```bash
cd Source/KillSounds/code
dotnet build KillSounds.csproj -c Debug
```

构建成功后将 `KillSounds.dll` 自动复制到 `1.6/Assemblies/`。

## 版本

- 支持游戏版本：1.6
- 依赖：Harmony（工坊 ID 2009463077）
- 包 ID：`xhpp.KillSounds`
