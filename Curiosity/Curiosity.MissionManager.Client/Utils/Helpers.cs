using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MissionManager.Client.Utils
{
    public class Helpers
    {
        public static Vector3 GetRandomSpawnPosFromPlayer(Player player, float minDist, float maxDist)
        {
            Vector3 spawnPos = player.Character.GetOffsetPosition(new Vector3(GetRandomFloat(-minDist, minDist),
                GetRandomFloat(minDist, maxDist), 0f));
            spawnPos.Z = World.GetGroundHeight(spawnPos);
            return spawnPos;
        }

        public static bool IsPosShitSpawn(PlayerList playerList, Vector3 pos, float radius)
        {
            foreach (Player player in playerList)
                if (player.Character != null && World.GetDistance(player.Character.Position, pos) < radius)
                    return true;

            //foreach (Safezones.Safezone safezone in Safezones.SAFEZONES)
            //    if (World.GetDistance(pos, safezone.Pos) < safezone.Range)
            //        return true;

            return false;
        }

        public static int GetRandomInt(int end)
        {
            return GetRandomIntInRange(0, end);
        }

        public static int GetRandomInt(int start, int end)
        {
            return GetRandomIntInRange(start, end);
        }

        public static float GetRandomFloat(float end)
        {
            return GetRandomFloatInRange(0, end);
        }

        public static float GetRandomFloat(float start, float end)
        {
            return GetRandomFloatInRange(start, end);
        }
    }
}
