using System;
using HarmonyLib;
using Verse;
using Verse.Sound;

namespace KillSounds
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        public const string HarmonyId = "xhpp.killsounds";

        private static readonly string[] ExpectedSoundDefs =
        {
            "KillSoundPool_All",
            "KillSoundPool_NoScream",
            "KillSoundPool_Battlefield",
            "KillSound2_CF",
            "KillSound3_CF",
            "KillSound4_CF",
            "KillSound5_CF",
            "KillSound6_CF"
        };

        static HarmonyInit()
        {
            Log.Message("KillSounds_Loading".Translate());

            try
            {
                Harmony harmony = new Harmony(HarmonyId);
                harmony.PatchAll();

                ValidateSoundDefs();

                Log.Message("KillSounds_Loaded".Translate());
            }
            catch (Exception ex)
            {
                Log.Error("KillSounds_PatchFailed".Translate(ex.ToString()));
            }
        }

        private static void ValidateSoundDefs()
        {
            foreach (string name in ExpectedSoundDefs)
            {
                if (DefDatabase<SoundDef>.GetNamedSilentFail(name) == null)
                    Log.Warning("KillSounds_MissingSound".Translate(name));
            }
        }
    }
}
