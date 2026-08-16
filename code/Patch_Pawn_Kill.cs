using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace KillSounds
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_Pawn_Kill
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            try
            {
                Settings settings = Mod.Settings;
                if (!settings.modEnabled || !TryGetKiller(dinfo, out Pawn instigator))
                    return;

                if (!ShouldTrigger(settings, instigator, __instance))
                    return;

                int currentTick = Find.TickManager.TicksGame;
                int streakWindowTicks = settings.streakTimeLimitSeconds.SecondsToTicks();
                int? streakCount = StreakTracker.RecordKill(instigator, __instance, currentTick, streakWindowTicks);
                if (!streakCount.HasValue)
                    return;

                if (settings.debugLogging)
                    Log.Message("KillSounds_Log_Selected".Translate());

                SoundDef sound = SoundPicker.ChooseSound(settings, streakCount.Value);
                if (sound != null)
                {
                    if (settings.debugLogging)
                        Log.Message("KillSounds_Log_Playing".Translate());

                    sound.PlayOneShot(SoundInfo.InMap(new TargetInfo(__instance)));
                }

                VisualFeedback.Trigger(settings, instigator, streakCount.Value);
            }
            catch (System.Exception ex)
            {
                Log.Error("KillSounds_PlayError".Translate(ex.ToString()));
            }
        }

        private static bool TryGetKiller(DamageInfo? dinfo, out Pawn instigator)
        {
            instigator = null;
            if (!dinfo.HasValue)
                return false;

            instigator = dinfo.Value.Instigator as Pawn;
            return instigator != null && instigator.Faction != null;
        }

        private static bool ShouldTrigger(Settings settings, Pawn instigator, Pawn victim)
        {
            if (victim.Faction == null || victim.Faction.IsPlayer)
                return false;

            if (settings.allyTrigger)
            {
                bool instigatorOnPlayerSide = instigator.Faction.IsPlayer || !instigator.Faction.HostileTo(Faction.OfPlayer);
                return instigatorOnPlayerSide && victim.Faction.HostileTo(Faction.OfPlayer);
            }

            return instigator.Faction.IsPlayer;
        }
    }
}
