using RimWorld;
using UnityEngine;
using Verse;

namespace KillSounds
{
    public static class VisualFeedback
    {
        public static void Trigger(Settings settings, Pawn killer, int streakCount)
        {
            if (!settings.enableVisualFeedback)
                return;

            if (killer == null)
                return;

            if (!killer.Spawned)
                return;

            if (killer.Map == null)
                return;

            if (settings.enableStreakText && streakCount >= 2)
                ThrowStreakText(killer.Map, killer.DrawPos + Vector3.up * 1.5f, streakCount);
        }

        private static void ThrowStreakText(Map map, Vector3 pos, int streak)
        {
            int level = Mathf.Clamp(streak, 2, 6);
            string text = ("KillSounds_StreakText_" + level).Translate();
            MoteMaker.ThrowText(pos, map, text, StreakColor(level), 1.2f);
        }

        private static Color StreakColor(int level)
        {
            switch (level)
            {
                case 2:
                    return new Color(1f, 0.92f, 0.25f);
                case 3:
                    return new Color(1f, 0.62f, 0.12f);
                case 4:
                    return new Color(1f, 0.45f, 0.1f);
                case 5:
                case 6:
                    return new Color(1f, 0.2f, 0.15f);
                default:
                    return Color.white;
            }
        }
    }
}
