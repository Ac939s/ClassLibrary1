using UnityEngine;
using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.Sound;


namespace Xhpp_KillSounds
{
    public class KillSoundSettings : ModSettings
    {
        public bool enableKillSound = false; //模组开关
        public bool enableKillSound2 = true; //盟友也会触发音效开关
        public bool enableKillSound3 = false; //是否触发惨叫
        public bool enableKillSound4 = true; //是否触发连杀
        public bool enableKillSound5 = false; //是否仅战地

        public float killStreakTimeLimit = 10f; // 秒
        public int lastKillTick = -60000; //连杀计时器
        public int KillCount = 0; //连杀计时器

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableKillSound, "enableKillSound", false);
            Scribe_Values.Look(ref enableKillSound2, "enableKillSound2", true);
            Scribe_Values.Look(ref enableKillSound3, "enableKillSound3", false);
            Scribe_Values.Look(ref enableKillSound4, "enableKillSound4", true);
            Scribe_Values.Look(ref enableKillSound5, "enableKillSound5", false);
        }
    }

    public class colseKillSound : Mod
    {
        public static KillSoundSettings settings;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("关闭本模组", ref settings.enableKillSound, "勾选关闭");
            listing_Standard.CheckboxLabeled("是否盟友也会触发击杀音效", ref settings.enableKillSound2, "勾选后盟友也会触发");
            listing_Standard.CheckboxLabeled("是否触发惨叫", ref settings.enableKillSound3, "勾选会触发");
            listing_Standard.CheckboxLabeled("是否触发连杀", ref settings.enableKillSound4, "勾选会触发");
            listing_Standard.CheckboxLabeled("是否仅战地", ref settings.enableKillSound5, "勾是后仅播放战地音效");
            listing_Standard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "模组配置";
        }

        public colseKillSound(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<KillSoundSettings>();
        }
    }

    [DefOf]
    public static class audioRef
    {
        //普通
        public static SoundDef KillSound1;
        public static SoundDef KillSound2;
        public static SoundDef KillSound3;
        public static SoundDef KillSound4;
        public static SoundDef KillSound5;
        public static SoundDef KillSound6;
        public static SoundDef KillSound7;
        public static SoundDef KillSound8;
        public static SoundDef KillSound9;
        public static SoundDef KillSound10;
        public static SoundDef KillSound11;
        public static SoundDef KillSound12;
        public static SoundDef KillSound13;
        public static SoundDef KillSound14;
        public static SoundDef KillSound15;

        //连杀
        public static SoundDef KillSound2_CF;
        public static SoundDef KillSound3_CF;
        public static SoundDef KillSound4_CF;
        public static SoundDef KillSound5_CF;
        public static SoundDef KillSound6_CF;

        static audioRef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(audioRef));
        }
    }

    [StaticConstructorOnStartup]
    public static class SoundSets
    {
        public static readonly SoundDef[] AllKillSounds;
        public static readonly SoundDef[] NoSpecForceKillSounds;
        public static readonly SoundDef[] BattleFieldKillSounds;

        static SoundSets()
        {
            AllKillSounds = new[]
            {
                audioRef.KillSound1,
                audioRef.KillSound2,
                audioRef.KillSound3,
                audioRef.KillSound4,
                audioRef.KillSound5,
                audioRef.KillSound6,
                audioRef.KillSound7,
                audioRef.KillSound8,
                audioRef.KillSound9,
                audioRef.KillSound10,
                audioRef.KillSound11,
                audioRef.KillSound12,
                audioRef.KillSound13,
                audioRef.KillSound14,
                audioRef.KillSound15
            };

            NoSpecForceKillSounds = new[]
            {
                audioRef.KillSound1,
                audioRef.KillSound2,
                audioRef.KillSound3,
                audioRef.KillSound4,
                audioRef.KillSound8,
                audioRef.KillSound9,
                audioRef.KillSound10,
                audioRef.KillSound11
            };

            BattleFieldKillSounds = new[]
            {
                audioRef.KillSound1,
                audioRef.KillSound2,
                audioRef.KillSound9,
                audioRef.KillSound10,
                audioRef.KillSound11
            };
        }
    }

    [StaticConstructorOnStartup]
    public class StartUp
    {
        static StartUp()
        {
            Log.Message("Kill Sounds Mod 加载中...");
            try
            {
                var harmony = new Harmony("xhpp.killsounds");
                harmony.PatchAll();
                Log.Message("[Kill Sounds] Mod 已加载");

                // 预检音效文件
                string[] names =
                {
                    "KillSound1", "KillSound2", "KillSound3", "KillSound4",
                    "KillSound5", "KillSound6", "KillSound7", "KillSound8",
                    "KillSound9", "KillSound10", "KillSound11", "KillSound12",
                    "KillSound13", "KillSound14", "KillSound15",
                    "KillSound2_CF", "KillSound3_CF", "KillSound4_CF", "KillSound5_CF", "KillSound6_CF"
                };

                foreach (var n in names)
                {
                    var sd = DefDatabase<SoundDef>.GetNamedSilentFail(n);
                    if (sd == null)
                        Log.Warning($"[Kill Sounds] 缺少 SoundDef: {n}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Kill Sounds] Harmony 补丁应用失败: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            try
            {
                // 安全检查or模组是否启用
                if (__instance == null || colseKillSound.settings.enableKillSound) return;

                int currentTick = Find.TickManager.TicksGame;
                // 检查凶手条件
                if (dinfo.HasValue && //伤害详情有值
                    dinfo.Value.Instigator is Pawn instigator && //伤害对象属于pawn
                    instigator.Faction != null && // 凶手有阵营
                    __instance.Faction != null && // 受害者有阵营
                    !__instance.Faction.IsPlayer) // 受害者不是玩家阵营
                {
                    // 判断是否触发音效
                    bool shouldTrigger = false;

                    if (colseKillSound.settings.enableKillSound2)
                    {
                        // 开关开启：盟友也会触发
                        shouldTrigger =
                            (instigator.Faction.IsPlayer || !instigator.Faction.HostileTo(Faction.OfPlayer)) &&
                            __instance.Faction.HostileTo(Faction.OfPlayer);
                    }
                    else
                    {
                        // 开关关闭：仅凶手是玩家才触发
                        shouldTrigger = instigator.Faction.IsPlayer;
                    }

                    if (!shouldTrigger) return;

                    Log.Message("1 挑选Pawn_Kill");


                    // 检查是否在时间限制内
                    if (currentTick - colseKillSound.settings.lastKillTick <=
                        colseKillSound.settings.killStreakTimeLimit * 60)
                    {
                        colseKillSound.settings.KillCount++;
                        Log.Message("连杀数" + colseKillSound.settings.KillCount);
                    }
                    else
                    {
                        colseKillSound.settings.KillCount = 1;
                    }

                    // 更新状态
                    colseKillSound.settings.lastKillTick = currentTick;
                    Log.Message("时间刻:" + currentTick);
                    // 获取对应的连杀音效（KillSound2_CF 对应 2连杀，以此类推）
                    SoundDef chosen = GetStreakKillSound(colseKillSound.settings.KillCount);

                    if (chosen != null)
                    {
                        Log.Message("2 挑选Pawn_Kill - 准备播放音效");
                        TryPlaySound(chosen, __instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[Kill Sounds] 播放音效时出错: {ex}");
            }
        }

        // 获取对应连杀数的音效
        private static SoundDef GetStreakKillSound(int streakCount)
        {
            var soundSets = SoundSets.NoSpecForceKillSounds; //不包含惨叫其余音频
            if (colseKillSound.settings.enableKillSound5)
            {
                soundSets = SoundSets.BattleFieldKillSounds; //仅战地
            }
            else if (colseKillSound.settings.enableKillSound3)
            {
                soundSets = SoundSets.AllKillSounds; //包含惨叫所有音频（没连杀的）
            }

            if (colseKillSound.settings.enableKillSound4)
            {
                switch (streakCount)
                {
                    case 1:
                        return Rand.Element(soundSets);
                    case 2:
                        return audioRef.KillSound2_CF;
                    case 3:
                        return audioRef.KillSound3_CF;
                    case 4:
                        return audioRef.KillSound4_CF;
                    case 5:
                        return audioRef.KillSound5_CF;
                    case 6:
                        return audioRef.KillSound6_CF;
                    default:
                        return Rand.Element(soundSets); // 7+也走普通音效
                }
            }

            return Rand.Element(soundSets);
        }

        private static void TryPlaySound(SoundDef soundDef, Pawn pawn)
        {
            Log.Message("播放音效");
            if (soundDef == null || pawn == null) return;
            soundDef.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn)));
        }
    }
}