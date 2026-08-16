using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KillSounds
{
    public static class StreakTracker
    {
        private class StreakInfo
        {
            public int killCount;
            public int lastKillTick;
            public int lastVictimId = -1;
        }

        private static readonly Dictionary<int, StreakInfo> streakByKiller = new Dictionary<int, StreakInfo>();
        private const int PruneInterval = 256;
        private static int callsSincePrune;

        public static int? RecordKill(Pawn killer, Pawn victim, int currentTick, int streakWindowTicks)
        {
            StreakInfo streak = GetOrCreate(killer);

            if (streak.lastVictimId == victim.thingIDNumber)
                return null;

            if (currentTick - streak.lastKillTick <= streakWindowTicks)
                streak.killCount++;
            else
                streak.killCount = 1;

            streak.lastKillTick = currentTick;
            streak.lastVictimId = victim.thingIDNumber;

            MaybePrune(currentTick, streakWindowTicks);

            return streak.killCount;
        }

        private static StreakInfo GetOrCreate(Pawn killer)
        {
            int id = killer.thingIDNumber;
            StreakInfo streak;
            if (!streakByKiller.TryGetValue(id, out streak))
            {
                streak = new StreakInfo();
                streakByKiller[id] = streak;
            }
            return streak;
        }

        private static void MaybePrune(int currentTick, int streakWindowTicks)
        {
            if (++callsSincePrune < PruneInterval)
                return;

            callsSincePrune = 0;

            int staleBefore = currentTick - streakWindowTicks * 2;
            List<int> staleIds = null;

            foreach (KeyValuePair<int, StreakInfo> pair in streakByKiller)
            {
                if (pair.Value.lastKillTick < staleBefore)
                {
                    if (staleIds == null)
                        staleIds = new List<int>();
                    staleIds.Add(pair.Key);
                }
            }

            if (staleIds == null)
                return;

            foreach (int id in staleIds)
                streakByKiller.Remove(id);
        }
    }
}
