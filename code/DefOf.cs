using RimWorld;
using Verse;
using Verse.Sound;

namespace KillSounds
{
    [RimWorld.DefOf]
    public static class DefOf
    {
        public static SoundDef KillSoundPool_All;
        public static SoundDef KillSoundPool_NoScream;
        public static SoundDef KillSoundPool_Battlefield;

        public static SoundDef KillSound2_CF;
        public static SoundDef KillSound3_CF;
        public static SoundDef KillSound4_CF;
        public static SoundDef KillSound5_CF;
        public static SoundDef KillSound6_CF;

        static DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DefOf));
        }
    }
}
