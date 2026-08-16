using Verse;
using Verse.Sound;

namespace KillSounds
{
    public static class SoundPicker
    {
        public static SoundDef ChooseSound(Settings settings, int streakCount)
        {
            SoundDef pool = settings.battlefieldOnly
                ? DefOf.KillSoundPool_Battlefield
                : settings.enableScream
                    ? DefOf.KillSoundPool_All
                    : DefOf.KillSoundPool_NoScream;

            if (settings.enableStreak)
            {
                switch (streakCount)
                {
                    case 2:
                        return DefOf.KillSound2_CF;
                    case 3:
                        return DefOf.KillSound3_CF;
                    case 4:
                        return DefOf.KillSound4_CF;
                    case 5:
                        return DefOf.KillSound5_CF;
                    case 6:
                        return DefOf.KillSound6_CF;
                }
            }

            return pool;
        }
    }
}
