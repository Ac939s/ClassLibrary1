using UnityEngine;
using Verse;

namespace KillSounds
{
    public class Mod : Verse.Mod
    {
        public static Settings Settings { get; private set; }

        public Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "KillSounds_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DrawSettings(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
