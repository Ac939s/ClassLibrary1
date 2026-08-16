using UnityEngine;
using Verse;

namespace KillSounds
{
    public class Settings : ModSettings
    {
        public bool modEnabled = true;
        public bool allyTrigger = true;
        public bool enableScream = false;
        public bool enableStreak = true;
        public bool battlefieldOnly = false;
        public float streakTimeLimitSeconds = 10f;
        public bool debugLogging = false;
        public bool enableVisualFeedback = true;
        public bool enableStreakText = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref modEnabled, "modEnabled", true);
            Scribe_Values.Look(ref allyTrigger, "allyTrigger", true);
            Scribe_Values.Look(ref enableScream, "enableScream", false);
            Scribe_Values.Look(ref enableStreak, "enableStreak", true);
            Scribe_Values.Look(ref battlefieldOnly, "battlefieldOnly", false);
            Scribe_Values.Look(ref streakTimeLimitSeconds, "streakTimeLimitSeconds", 10f);
            Scribe_Values.Look(ref debugLogging, "debugLogging", false);
            Scribe_Values.Look(ref enableVisualFeedback, "enableVisualFeedback", true);
            Scribe_Values.Look(ref enableStreakText, "enableStreakText", true);
        }

        public void DrawSettings(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled(
                "KillSounds_EnableMod".Translate(),
                ref modEnabled,
                "KillSounds_EnableMod_Tip".Translate());

            if (modEnabled)
            {
                listing.Gap(8f);

                listing.CheckboxLabeled(
                    "KillSounds_AllyTrigger".Translate(),
                    ref allyTrigger,
                    "KillSounds_AllyTrigger_Tip".Translate());

                listing.CheckboxLabeled(
                    "KillSounds_Scream".Translate(),
                    ref enableScream,
                    "KillSounds_Scream_Tip".Translate());

                listing.CheckboxLabeled(
                    "KillSounds_Streak".Translate(),
                    ref enableStreak,
                    "KillSounds_Streak_Tip".Translate());

                if (enableStreak)
                {
                    listing.SliderLabeled(
                        "KillSounds_StreakTime".Translate(streakTimeLimitSeconds.ToString("0")),
                        streakTimeLimitSeconds,
                        1f,
                        30f,
                        tooltip: "KillSounds_StreakTime_Tip".Translate());
                }

                listing.CheckboxLabeled(
                    "KillSounds_Battlefield".Translate(),
                    ref battlefieldOnly,
                    "KillSounds_Battlefield_Tip".Translate());

                listing.Gap(8f);

                listing.CheckboxLabeled(
                    "KillSounds_DebugLog".Translate(),
                    ref debugLogging,
                    "KillSounds_DebugLog_Tip".Translate());

                listing.Gap(8f);

                listing.Label("KillSounds_VisualHeader".Translate());

                listing.CheckboxLabeled(
                    "KillSounds_VisualFeedback".Translate(),
                    ref enableVisualFeedback,
                    "KillSounds_VisualFeedback_Tip".Translate());

                if (enableVisualFeedback)
                {
                    listing.CheckboxLabeled(
                        "KillSounds_StreakText".Translate(),
                        ref enableStreakText,
                        "KillSounds_StreakText_Tip".Translate());
                }
            }

            listing.End();
        }
    }
}
