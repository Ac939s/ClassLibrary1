# KillSounds 扩展设计

> 状态：待审阅
> 范围：触发条件多样化、新事件类型、视觉反馈、设置界面重构

## 1. 目标

在现有"击杀播放音效 + 连杀音效"基础上，扩展为**事件驱动**的击杀反馈系统：

- 区分不同类型的击杀（爆头/处刑/爆炸/火焰/误杀），播放对应音效与视觉反馈
- 触发条件从硬编码派系判断改为**可配置的组合条件**
- 新增视觉反馈（浮动文字、屏幕震动）
- 设置界面重构为**分组 Tab**，容纳更多配置

## 2. 现状分析

当前管线（`code/Patch_Pawn_Kill.cs`）：

```
Pawn.Kill → Postfix
  → Settings 检查
  → TryGetKiller（dinfo.Instigator as Pawn + Faction != null）
  → ShouldTrigger（硬编码派系判断）
  → StreakTracker.RecordKill（连杀计数）
  → SoundPicker.ChooseSound（固定 3 池 + 2~6 连杀音效）
  → sound.PlayOneShot
```

问题：
- 触发判定写死，无法扩展（武器/目标类型/伤害类型等）
- 音效选择只有"普通池 + 连杀"两种维度，无事件概念
- 无视觉反馈
- 设置项是平面 Checkbox 列表，再多就堆不下了

## 3. 总体架构改造

引入**击杀事件上下文** `KillContext`，重构为新管线：

```
Pawn.Kill → Postfix
  → KillContext.Build(dinfo, victim)          // 采集上下文
  → KillEventType.Detect(ctx)                 // 判定事件类型
  → TriggerRules.Passes(settings, ctx, evt)   // 组合条件过滤
  → StreakTracker.RecordKill(...)             // 连杀（保留现有）
  → SoundPicker.ChooseSound(settings, evt, streak)
  → VisualFeedback.Trigger(settings, ctx, evt, streak)  // 新增
  → sound.PlayOneShot
```

### 3.1 KillContext（新文件 `KillContext.cs`）

```csharp
public class KillContext
{
    public Pawn killer;
    public Pawn victim;
    public DamageInfo? dinfo;
    public DamageDef damageDef;      // dinfo.Def
    public ThingDef weapon;          // dinfo.Weapon
    public BodyPartRecord hitPart;   // dinfo.HitPart
    public Map map;
    public IntVec3 position;         // victim.Position
    public int tick;                 // Find.TickManager.TicksGame
    public bool oneHitKill;          // 近似：dinfo.Amount >= victim 最大生命值
}
```

要点：
- `Build()` 集中采集，后续模块只读 `KillContext`，不再各自重复取 `dinfo`
- `oneHitKill` 为近似判定（一击伤害 ≥ 最大生命值，即"过量击杀"），文档中注明局限

## 4. 事件类型（新文件 `KillEventType.cs`）

### 4.1 枚举与优先级

```csharp
public enum KillEventType
{
    Normal,        // 普通击杀（默认池，兜底）
    FriendlyFire,  // 误杀：同阵营/我方互杀
    Execution,     // 处刑
    Headshot,      // 爆头/颈部击杀
    Explosion,     // 爆炸击杀
    Fire,          // 火焰/烧伤致死
}
```

### 4.2 检测规则（优先级从高到低，互斥）

| 事件 | 规则 |
| --- | --- |
| `FriendlyFire` | 击杀者与受害者同派系，且该派系与玩家非敌对（玩家方内讧） |
| `Execution` | `dinfo.Def == DamageDefOf.ExecutionCut` |
| `Fire` | `dinfo.Def ∈ {Flame, Burn, AcidBurn, ElectricalBurn, Vaporize}` |
| `Explosion` | `dinfo.Def ∈ {Bomb, MiningBomb}`（1.5+ 爆炸伤害） |
| `Headshot` | `hitPart` 为 Head/Neck（优先于 Normal，且非上述类型） |
| `Normal` | 兜底 |

注：`Headshot` 依赖 `dinfo.HitPart`，爆炸/火焰等击杀该字段可能为 null —— 检测时先判爆炸/火焰再判爆头，天然规避。

### 4.3 音效池映射

为每个事件类型新增一个音效池 SoundDef（v1 复用现有 .ogg 素材，仅重组剪辑）：

| defName | 用途 |
| --- | --- |
| `KillSoundPool_Headshot` | 爆头击杀池 |
| `KillSoundPool_Execution` | 处刑击杀池 |
| `KillSoundPool_Explosion` | 爆炸击杀池 |
| `KillSoundPool_Fire` | 火焰击杀池 |
| `KillSoundPool_FriendlyFire` | 误杀池 |

默认音效池仍为：`KillSoundPool_All` / `NoScream` / `Battlefield`（受尖叫、战地选项控制）。

### 4.4 选择逻辑（`SoundPicker.cs` 重构）

```
if (streak 2~6 && enableStreak) → 连杀音效（优先级最高，现有行为保留）
else if (事件类型有专属池)      → 事件池
else                            → 普通池（受 scream/battlefield 选项）
```

## 5. 触发条件系统（新文件 `TriggerRules.cs`）

替代 `Patch_Pawn_Kill.ShouldTrigger`，由设置驱动。默认值保持现有行为（向后兼容）。

### 5.1 击杀者过滤（替代现有 `allyTrigger`）

```csharp
public enum KillerFilter { PlayerOnly, AllyIncludingPlayer, AnyNonHostileToPlayer }
```

| 值 | 含义 | 对应现状 |
| --- | --- | --- |
| `PlayerOnly` | 仅玩家阵营击杀触发 | `allyTrigger = false` |
| `AllyIncludingPlayer` | 玩家或友军击杀触发（默认） | `allyTrigger = true` |
| `AnyNonHostileToPlayer` | 任何不与玩家敌对者击杀触发 | —（扩展） |

### 5.2 受害者过滤

```csharp
public enum VictimFilter { EnemyOfPlayer, AnyHostileToPlayer, Any }
```

默认 `EnemyOfPlayer`（仅敌对派系目标，同现状：排除玩家派系）。

### 5.3 物种过滤（新增）

```csharp
public enum SpeciesFilter { Any, Humanlike, Animal, Mechanoid, Insect }
```

- `targetSpecies`：受害者物种（`victim.RaceProps.Humanlike/Animal/Insect/IsMechanoid`）
- `killerSpecies`：击杀者物种（同上）

### 5.4 武器过滤（新增）

```csharp
public enum WeaponClassFilter { Any, Melee, Ranged }
```

判定：`dinfo.Weapon` 存在且有远程能力 → Ranged；否则按近战（肉搏/近战武器）。

### 5.5 一击必杀（新增）

`oneHitKillOnly: bool` —— 仅在一击致死时触发（用 `KillContext.oneHitKill`）。

## 6. 视觉反馈（新文件 `VisualFeedback.cs`）

依赖已核实的 API：

- 浮动文字：`RimWorld.MoteMaker.ThrowText(Vector3, Map, string, Color, float)`
- 屏幕震动：`Find.CameraDriver.shaker.DoShake(float mag)`（或 `(mag, durationTicks)`）

### 6.1 连杀文字

| 连击 | 文字 key | 颜色 | 震屏 |
| --- | --- | --- | --- |
| 2 | `KillSounds_StreakText_2` | 黄 | 无 |
| 3 | `KillSounds_StreakText_3` | 橙 | 无 |
| 4 | `KillSounds_StreakText_4` | 深橙 | 无 |
| 5 | `KillSounds_StreakText_5` | 红 | 小 |
| 6+ | `KillSounds_StreakText_6` | 红 | 中 |

文字显示于受害者死亡位置，`overrideTimeBeforeStartFadeout` 约 1.2s。

### 6.2 事件震屏（可选）

| 事件 | 震屏幅度 |
| --- | --- |
| `Explosion` | 0.3 |
| `Fire` | 0.15 |
| `Execution` | 0.2 |

### 6.3 开关

- `enableVisualFeedback`：总开关
- `enableStreakText` / `enableScreenShake`：子开关
- `shakeMagnitudeScale`（0~2，默认 1）：震屏强度倍率

## 7. 设置界面重构（`Settings.cs` / `Mod.cs`）

用 `Verse.TabDrawer` 实现分组 Tab（`TabRecord` 列表），每个 Tab 内仍用 `Listing_Standard`：

| Tab | 内容 |
| --- | --- |
| 常规 | 启用模组、调试日志 |
| 触发 | 击杀者过滤、受害者过滤、物种过滤、武器过滤、一击必杀 |
| 事件 | 每个事件类型：音效开/关 + 视觉开/关（2 列网格） |
| 音效 | 尖叫开关、战地池开关、连杀开关、连杀时间窗（滑块） |
| 视觉 | 视觉总开关、连杀文字、震屏、震屏强度 |

### 7.1 Settings 字段（分组）

```csharp
public class Settings : ModSettings
{
    // 常规
    public bool modEnabled = true;
    public bool debugLogging = false;

    // 触发
    public KillerFilter killerFilter = KillerFilter.AllyIncludingPlayer;
    public VictimFilter victimFilter = VictimFilter.EnemyOfPlayer;
    public SpeciesFilter targetSpecies = SpeciesFilter.Any;
    public SpeciesFilter killerSpecies = SpeciesFilter.Any;
    public WeaponClassFilter weaponClass = WeaponClassFilter.Any;
    public bool oneHitKillOnly = false;

    // 事件开关（音效/视觉）
    public bool evtSoundHeadshot = true;    public bool evtVisualHeadshot = true;
    public bool evtSoundExecution = true;   public bool evtVisualExecution = true;
    public bool evtSoundExplosion = true;   public bool evtVisualExplosion = true;
    public bool evtSoundFire = true;        public bool evtVisualFire = true;
    public bool evtSoundFriendlyFire = true;public bool evtVisualFriendlyFire = true;

    // 音效
    public bool enableScream = false;
    public bool battlefieldOnly = false;
    public bool enableStreak = true;
    public float streakTimeLimitSeconds = 10f;

    // 视觉
    public bool enableVisualFeedback = true;
    public bool enableStreakText = true;
    public bool enableScreenShake = true;
    public float shakeMagnitudeScale = 1f;
}
```

所有字段经 `Scribe_Values.Look` 持久化（保留原 key 以兼容旧存档设置）。

## 8. 文件变更清单

### 新增（code/）
| 文件 | 职责 |
| --- | --- |
| `KillContext.cs` | 击杀事件上下文 |
| `KillEventType.cs` | 事件枚举 + 检测 + 事件池 DefOf 引用 |
| `TriggerRules.cs` | 组合条件过滤 |
| `VisualFeedback.cs` | 浮动文字 + 震屏 |

### 修改（code/）
| 文件 | 变更 |
| --- | --- |
| `Patch_Pawn_Kill.cs` | 重构为新管线，删除 `ShouldTrigger`/`TryGetKiller` 逻辑（移入 `TriggerRules`/`KillContext`） |
| `Settings.cs` | 字段分组 + Tab 界面 |
| `SoundPicker.cs` | 支持事件池选择 |
| `DefOf.cs` | 新增 5 个事件池引用 |
| `HarmonyInit.cs` | 扩展音效 Def 校验列表 |
| `KillSounds.csproj` | 登记新 `.cs` 文件 |

### 资源
| 路径 | 变更 |
| --- | --- |
| `1.6/Defs/SoundDefs.xml` | 新增 5 个事件音效池（复用现有 .ogg） |
| `Languages/English/keyed/` | 新增事件/Tab/视觉文字翻译 |
| `Languages/ChineseSimplified/Keyed/` | 同上（简中） |

## 9. 实现顺序（里程碑）

1. **M1 管线重构**：新增 `KillContext` + 提取 `TriggerRules`，行为与现状完全一致，构建验证无回归
2. **M2 事件类型**：`KillEventType` 检测 + 5 个事件池 Def + `SoundPicker` 支持
3. **M3 触发条件**：新设置字段 + `TriggerRules` 完整实现
4. **M4 视觉反馈**：`VisualFeedback` + 连杀文字 + 震屏
5. **M5 设置界面**：Tab 化重构
6. **M6 资源**：翻译、README/文档更新

每个里程碑独立可构建、可测试。

## 10. 风险与兼容性

| 风险 | 对策 |
| --- | --- |
| `dinfo.HitPart` 在爆炸/火焰击杀时为 null | 检测顺序先爆炸/火焰再爆头，天然规避 |
| 一击必杀判定为近似值 | 文档注明局限，用"过量伤害 ≥ 最大生命值"近似 |
| 设置界面旧存档字段缺失 | `Scribe_Values.Look` 带默认值，自动兼容 |
| 事件池音效素材重复 | v1 复用现有 .ogg 仅重组，后续可换素材 |
| Harmony 补丁顺序/性能 | 仍为单 Postfix，仅增加轻量判定，无新增补丁点 |
