# 编码规范（C# / RimWorld 模组）

> KillSounds 项目编码规范。所有新增/修改代码遵循本规范与现有 `code/` 文件风格。

## 语言与框架

- C#，目标框架 .NET Framework 4.7.2
- 引用：`Assembly-CSharp.dll`、`UnityEngine*.dll`、`0Harmony.dll`
- 主命名空间：`KillSounds`；常用 `using Verse;` / `using RimWorld;` / `using HarmonyLib;`

## 命名

| 类型 | 规则 | 示例 |
|------|------|------|
| 类 / 结构体 / 枚举 / 接口 | PascalCase | `SoundPicker`、`StreakTracker` |
| 方法 / 属性 / 事件 | PascalCase | `ChooseSound`、`RecordKill` |
| 私有字段 | camelCase | `streakByKiller`、`callsSincePrune` |
| 常量 | PascalCase | `PruneInterval` |
| 局部变量 / 方法参数 | camelCase | `sound`、`streakCount` |
| 私有嵌套类 | PascalCase | `StreakInfo` |

## 文件组织

- 一个公共类一个文件，文件名与类名一致，放在 `code/`
- 新增文件需在 `code/KillSounds.csproj` 的 `<Compile Include>` 中登记
- 遵循 RimWorld/Harmony 惯例：`[HarmonyPatch]`、`[StaticConstructorOnStartup]`、`[DefOf]`、`ModSettings` 子类、`Mod` 子类

## 代码风格

- 项目现有代码无注释惯例 → 不主动加注释；确需注释用中文简述
- 方法避免过长（~50 行以上考虑拆分）
- 不引入方案外的依赖或抽象
- 参考现有 `code/` 文件的写法与命名习惯

## 构建

```bash
dotnet build code/KillSounds.csproj -c Debug
```

- 在仓库根目录运行
- 要求 0 编译错误（0 警告优先）
- 构建成功后 DLL 自动复制到模组 `1.6/Assemblies/`

## RimWorld API 查询

- 用 RimSearcher MCP 工具（locate / inspect / read_code / trace / search_regex）查 Verse/RimWorld 类型与 Def，不要凭记忆猜 API
